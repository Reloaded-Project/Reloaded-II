using System.Runtime;
using Reloaded.Memory.Pointers;
using System.Security.Policy;
using System.Text.Json.Serialization;
using static Reloaded.Mod.Loader.Utilities.DRMHelper;
using static Reloaded.Mod.Loader.Utilities.Native.Kernel32;
using Console = System.Console;
using Environment = Reloaded.Mod.Shared.Environment;

namespace Reloaded.Mod.Loader;

/// <summary>
/// Provides an entry point to this .NET Core application for the native C++ bootstrapper.
/// </summary>
// ReSharper disable UnusedMember.Global
public static class EntryPoint
{
    // DO NOT RENAME THIS CLASS OR ITS PUBLIC METHODS
    private static Stopwatch _stopWatch;
    private static Loader _loader;
    private static ReloadedMappedFile _reloadedMappedFile;
    private static DelayInjector _delayInjector;
    private static Process _process = Process.GetCurrentProcess();
    private static SteamHook _steamHook;
    private static ProcessExitHook _exitHook;
    private static ProcessCrashHook _crashHook;
    private static EntryPointParameters _parameters;

    private static Logger Logger => _loader?.Logger;

    /* For ReadyToRun Feature */
    public static void Main() { } // Dummy for R2R images.

    /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

    /// <summary>
    /// Initializes the mod loader.
    /// Returns the port on the local machine (but that wouldn't probably be used).
    /// </summary>
    public static unsafe int Initialize(IntPtr argument, int argSize)
    {
        _reloadedMappedFile = new ReloadedMappedFile(_process.Id);
        SetupLoader((EntryPointParameters*)argument);
        _reloadedMappedFile.SetInitialized(true);
        
        // Force full GC. including LOH. Justification is most mods idle most of the time, so it may take long until Gen 2.
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.AddMemoryPressure(nint.MaxValue);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true); 
        GC.Collect(0, GCCollectionMode.Forced, false); // Invoke again to trigger some heuristic in GC telling it there's some pressure going on 
        GC.RemoveMemoryPressure(nint.MaxValue);
        
        return 0; // Server is no longer part of mod loader, return 0 port.
    }

    private static unsafe void SetupLoader(EntryPointParameters* parameters)
    {
        try
        {
            // Setup mod loader.
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            ExecuteTimed("Create Loader", CreateLoader);
            
            // Get Reloaded.Hooks
            try
            {
                _loader.LoadWithExportsIntoCurrentALC("reloaded.sharedlib.hooks");
            }
            catch (ReloadedException ex)
            {
                throw new Exception($"Failed to Load Reloaded Hooks Shared Lib Mod\n" +
                                    $"Quite honestly, if it's missing, this is kind of an achievement, please run the launcher, it'll download.\n" +
                                    $"{ex.Message}\n{ex.StackTrace}", ex);
            }
            
            SetupLoader2(parameters);
        }
        catch (Exception ex)
        {
            HandleLoadError(ex);
        }
    }

    // We must resolve hooks shared lib first, so we cannot inline this to avoid runtime exception from missing lib.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe void SetupLoader2(EntryPointParameters* parameters)
    {
        _loader.Manager.LoaderApi.GetController<IReloadedHooks>().TryGetTarget(out var hooks);

        var setupHooksTask = Task.Run(() => ExecuteTimed("Setting Up Hooks (Async)", () => SetupHooks(hooks)));
        InitialiseParameters(parameters);
        if (_parameters.SupportsDllPath && _parameters.DllPath != null)
        {
            var reloadedPath = Marshal.PtrToStringUni((nint)_parameters.DllPath);
            Logger!.LogWriteLineAsync($"Loaded via: {reloadedPath}", Logger.ColorInformation);
            if (reloadedPath!.EndsWith(".asi"))
                Logger!.LogWriteLineAsync($"Remove the above `.asi` file to uninstall. (Should be in your app/game folder)", Logger.ColorInformation);
        }
            
        ExecuteTimed("Loading Mods (Total)", () => LoadMods(hooks));

        setupHooksTask.Wait();
        Logger!.LogWriteLineAsync($"Total Loader Initialization Time: {_stopWatch.ElapsedMilliseconds}ms");
        _stopWatch.Reset();
    }

    // No inlining because we must merge this into current ALC first!!

    
    private static void CreateLoader() => _loader = new Loader();
    private static unsafe void InitialiseParameters(EntryPointParameters* parameters)
    {
        if (parameters != (void*)0)
        {
            if (!parameters->IsLatestVersion())
                Logger?.LogWriteLineAsync($"Bootstrapper (Reloaded.Mod.Loader.Bootstrapper.dll) is does not match expected version (Expected Version: {EntryPointParameters.CurrentVersion}, Actual Version: {parameters->Version}). Please upgrade the bootstrapper. If you are using ASI Loader re-deploy, otherwise copy Reloaded.Mod.Loader.Bootstrapper.dll.", Logger.ColorWarning);

            _parameters = EntryPointParameters.Copy(parameters);
        }
        else
        {
            Logger?.LogWriteLineAsync($"Expected EntryPointParameters but did not receive any. Bootstrapper (Reloaded.Mod.Loader.Bootstrapper.dll) is likely outdated. Please upgrade by copying a newer version of Reloaded.Mod.Loader.Bootstrapper.dll if integrating with another mod loader or re-deploy ASI Loader (if using ASI Loader).", Logger.ColorWarning);
        }
    }

    private static unsafe void SetupHooks(IReloadedHooks hooks)
    {
        // Hook ExitProcess to ensure log save on process exit.
        _exitHook = new ProcessExitHook(SaveAndFlushLog, hooks);
        _crashHook = new ProcessCrashHook(&HandleCrash, hooks);

        // Hook Console Close
        if (_loader.Console != null)
            _loader.Console.OnConsoleClose += SaveAndFlushLog;

        // Hook Steam
        _steamHook = new SteamHook(hooks, _loader.Logger, Path.GetDirectoryName(_process.MainModule.FileName));
    }

    private static void LoadMods(IReloadedHooks hooks)
    {
        // Note: If loaded externally, we assume another mod loader or DLL override took care of bypassing DRM.
        bool loadedFromExternalSource = (_parameters.Flags & EntryPointFlags.LoadedExternally) != 0;
        bool requiresDelayStart = false;
        DrmType drmTypes = DrmType.None;
        
        if (loadedFromExternalSource)
        {
            Logger?.LogWriteLineAsync($"Note: Reloaded is being loaded from an external source or mod loader.", Logger.ColorInformation);
        }
        else
        {
            try
            {
                var basicPeParser = new BasicPeParser(Environment.CurrentProcessLocation.Value);
                drmTypes = CheckDrmAndNotify(basicPeParser, _loader.Logger, out requiresDelayStart);
            }
            catch (Exception e)
            {
                Logger?.LogWriteLineAsync($"Failed to check DRM. Probably unable to read EXE file.", Logger.ColorWarning);
            }
        }

        if (!requiresDelayStart || loadedFromExternalSource)
        {
            _loader.LoadForCurrentProcess();
        }
        else
        {
            Logger?.LogWriteLineAsync($"DRM Requiring Delayed Initialization ({drmTypes}) Found.\n" +
                                      $"Reloaded will try to initialize late to bypass this DRM.\n" +
                                      $"Please note this feature is experimental.\n" +
                                      $"If you encounter issues, report and/or try ASI Loader `Edit Application -> Deploy ASI Loader`", Logger.ColorWarning);
                
            _delayInjector = new DelayInjector(hooks, () =>
            {
                Logger?.LogWriteLineAsync($"Loading via Delayed Injection (DRM Workaround)", Logger.ColorInformation);
                _loader.LoadForCurrentProcess();
            }, _loader.Logger);
        }
    }
        
    private static void SaveAndFlushLog()
    {
        Logger?.LogWriteLineAsync("ExitProcess Hook: Log End");
        Logger?.Shutdown();
        _loader?.LogWriter?.Flush();
    }

    private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = (Exception)e.ExceptionObject;
        var message = $"Unhandled Exception: {exception.Message}\n" +
                      $"Stack Trace: {exception.StackTrace}";

        Logger?.LogWriteLine(message, _loader.Logger.ColorError);
    }

    /* Utility Functions */

    /// <summary>
    /// Executes a given function and prints running stats of how long it took to execute.
    /// </summary>
    public static void ExecuteTimed(string text, Action action)
    {
        long initialTime = _stopWatch.ElapsedMilliseconds;
        action();
        Logger?.LogWriteLineAsync($"{text} | Time: {_stopWatch.ElapsedMilliseconds - initialTime}ms");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleLoadError(Exception ex)
    {
        // This method is singled out to avoid loading System.Windows.Forms at startup; because it is lazy loaded.
        var errorMessage = $"Failed to Load Reloaded-II.\n{ex.Message}\n{ex.StackTrace}\nA log is available at: {_loader?.LogWriter?.FlushPath}";

        Exception innerException = ex.InnerException;
        while (innerException != null)
        {
            errorMessage += $"\nInner Exception: {innerException.Message} | {innerException.StackTrace}";
            innerException = innerException.InnerException;
        }

        _loader?.Console?.WaitForConsoleInit();
        Logger?.LogWriteLine(errorMessage, Logger.ColorError);
        _loader?.LogWriter?.Flush();
        User32.MessageBox(0, errorMessage, "Oh Noes!", 0);
    }
    
    private static unsafe int HandleCrash(IntPtr exceptionPtrs)
    {
        var exceptionPointers = (EXCEPTION_POINTERS*)exceptionPtrs;

        try
        {
            var currentLogPath = _loader.LogWriter.FlushPath;

            // Determine Paths
            var logFolderName = Path.GetFileNameWithoutExtension(currentLogPath);
            var dumpFolder = Path.Combine(Paths.CrashDumpPath, logFolderName!);
            var dumpPath = Path.Combine(dumpFolder, "dump.dmp");
            var logPath = Path.Combine(dumpFolder, Path.GetFileName(currentLogPath)!);
            var infoPath = Path.Combine(dumpFolder, "info.json"!);
            Directory.CreateDirectory(dumpFolder);

            // Flush log.
            Logger?.LogWriteLine("Crashed. Generating Crash Dump. Might take a while.", Logger.ColorError);
            Logger?.Shutdown();
            _loader.LogWriter.Flush();
            _loader.LogWriter.Dispose();

            File.Copy(currentLogPath, logPath, true);

            // Let's create our crash dump.
            using var crashDumpFile = new FileStream(dumpPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var exceptionInfo = new Kernel32.MinidumpExceptionInformation()
            {
                ThreadId = Kernel32.GetCurrentThread(),
                ExceptionPointers = (IntPtr)exceptionPointers,
                ClientPointers = true
            };

            // 289 is the default for Minidumps made by Windows Error Reporting.
            // Figured this out by opening one in hex editor.
            Kernel32.MiniDumpWriteDump(_process.Handle, (uint)_process.Id, crashDumpFile.SafeFileHandle, (MinidumpType)289, ref exceptionInfo, IntPtr.Zero, IntPtr.Zero);

            // Inform the user.
            var info = new CrashDumpInfo(_loader, exceptionPointers);
            File.WriteAllText(infoPath, JsonSerializer.Serialize(info, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            }));
            User32.MessageBox(0, $"Crash information saved. If you believe this crash is mod related, ask for mod author(s) support. Otherwise, delete the generated files.\n\n" +
                                 $"Info for nerds:\n" +
                                 $"Crash Address: {info.CrashAddress}\n" +
                                 $"Faulting Module: {info.FaultingModulePath}", "Shit! Program Crashed in Application Code!", 0);
            
            Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{dumpFolder}\"")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            return (int)Kernel32.EXCEPTION_FLAG.EXCEPTION_EXECUTE_HANDLER;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Epic Fail! Failed to create crash dump.\n" +
                              $"{e.Message}\n" +
                              $"{e.StackTrace}");
            return 0;
        }
    }
}

// ReSharper restore UnusedMember.Global