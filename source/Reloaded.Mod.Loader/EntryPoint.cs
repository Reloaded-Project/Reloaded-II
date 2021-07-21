using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Utility.Parsers;
using Reloaded.Mod.Loader.Logging;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Utilities;
using Reloaded.Mod.Loader.Utilities.Native;
using Reloaded.Mod.Loader.Utilities.Steam;
using static Reloaded.Mod.Loader.Utilities.LogMessageFormatter;
using Environment = Reloaded.Mod.Shared.Environment;

namespace Reloaded.Mod.Loader
{
    /// <summary>
    /// Provides an entry point to this .NET Core application for the native C++ bootstrapper.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public static class EntryPoint
    {
        // DO NOT RENAME THIS CLASS OR ITS PUBLIC METHODS
        private static Stopwatch _stopWatch;
        private static Loader _loader;
        private static Host _server;
        private static MemoryMappedFile _memoryMappedFile;
        private static DelayInjector _delayInjector;
        private static Process _process;
        private static SteamHook _steamHook;
        private static ProcessExitHook _exitHook;
        private static EntryPointParameters _parameters;

        /* For ReadyToRun Feature */
        public static void Main() { } // Dummy for R2R images.

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Initializes the mod loader.
        /// Returns the port on the local machine (but that wouldn't probably be used).
        /// </summary>
        public static unsafe int Initialize(IntPtr argument, int argSize)
        {
            EnableProfileOptimization();

            // Write port as a Memory Mapped File, to allow Mod Loader's Launcher to discover the mod port.
            // (And to stop bootstrapper from loading loader again).
            _process = Process.GetCurrentProcess();
            int pid = _process.Id;
            _memoryMappedFile = MemoryMappedFile.CreateOrOpen(ServerUtility.GetMappedFileNameForPid(pid), sizeof(int));
            var view = _memoryMappedFile.CreateViewStream();
            var binaryWriter = new BinaryWriter(view);
            binaryWriter.Write((int)0);

            // Setup Loader
            SetupLoader((EntryPointParameters*) argument);

            // Only write port on completed initialization.
            // If port is 0, assume in loading state
            binaryWriter.Seek(-sizeof(int), SeekOrigin.Current);
            binaryWriter.Write(_server.Port);
            return _server?.Port ?? 0;
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
                InitialiseParameters(parameters);
                var createHostTask = Task.Run(() => ExecuteTimed("Create Loader Host (Async)", CreateHost));
                var setupHooksTask = Task.Run(() => ExecuteTimed("Setting Up Hooks (Async)", SetupHooks));
                ExecuteTimed("Loading Mods (Total)", LoadMods);

                setupHooksTask.Wait();
                createHostTask.Wait();
                _loader?.Logger?.WriteLineAsync(AddLogPrefix($"Total Loader Initialization Time: {_stopWatch.ElapsedMilliseconds}ms"));
                _stopWatch.Reset();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void CreateLoader() => _loader = new Loader();
        private static unsafe void InitialiseParameters(EntryPointParameters* parameters)
        {
            if (parameters != (void*)0)
            {
                if (!parameters->IsLatestVersion())
                    _loader?.Logger?.WriteLineAsync(AddLogPrefix($"Bootstrapper (Reloaded.Mod.Loader.Bootstrapper.dll) is does not match expected version (Expected Version: {EntryPointParameters.CurrentVersion}, Actual Version: {parameters->Version}). Please upgrade the bootstrapper. If you are using ASI Loader re-deploy, otherwise copy Reloaded.Mod.Loader.Bootstrapper.dll."), _loader.Logger.ColorYellow);

                _parameters = EntryPointParameters.Copy(parameters);
            }
            else
            {
                _loader?.Logger?.WriteLineAsync(AddLogPrefix($"Expected EntryPointParameters but did not receive any. Bootstrapper (Reloaded.Mod.Loader.Bootstrapper.dll) is likely outdated. Please upgrade by copying a newer version of Reloaded.Mod.Loader.Bootstrapper.dll if integrating with another mod loader or re-deploy ASI Loader (if using ASI Loader)."), _loader.Logger.ColorYellow);
            }
        }

        private static void CreateHost()   => _server = new Host(_loader);
        private static void SetupHooks()
        {
            // Hook ExitProcess to ensure log save on process exit.
            _exitHook = new ProcessExitHook(SaveAndFlushLog);

            // Hook Console Close
            if (_loader.Console != null)
                _loader.Console.OnConsoleClose += SaveAndFlushLog;

            // Hook Steam
            _steamHook = new SteamHook(ReloadedHooks.Instance, _loader.Logger, Path.GetDirectoryName(_process.MainModule.FileName));
        }

        private static void LoadMods()
        {
            // Check for Known DRM and Workarounds.
            var basicPeParser = new BasicPeParser(Environment.CurrentProcessLocation.Value);
            var drmTypes = DRMHelper.CheckDrmAndNotify(basicPeParser, _loader.Logger, out bool requiresDelayStart);
            
            // Note: If loaded externally, we assume another mod loader or DLL override took care of bypassing DRM.
            bool loadedFromExternalSource = (_parameters.Flags & EntryPointFlags.LoadedExternally) != 0;
            if (loadedFromExternalSource)
                _loader?.Logger?.WriteLineAsync(AddLogPrefix($"Note: Reloaded is being loaded from an external source or mod loader."), _loader.Logger.ColorGreen);

            if (!requiresDelayStart || loadedFromExternalSource)
            {
                _loader.LoadForCurrentProcess();
            }
            else
            {
                _loader?.Logger?.WriteLineAsync(AddLogPrefix($"DRM Requiring Delayed Initialization ({drmTypes}) Found.\n" +
                                                             $"Reloaded will try to initialize late to bypass this DRM.\n" +
                                                             $"Please note this feature is experimental."), _loader.Logger.ColorPinkLight);
                
                _delayInjector = new DelayInjector(() =>
                {
                    _loader?.Logger?.WriteLineAsync(AddLogPrefix($"Loading via Delayed Injection (DRM Workaround)"), _loader.Logger.ColorPinkLight);
                    _loader.LoadForCurrentProcess();
                }, _loader.Logger);
            }
        }
        
        private static void SaveAndFlushLog()
        {
            _loader?.Logger?.WriteLineAsync(AddLogPrefix("ExitProcess Hook: Log End"));
            _loader?.Logger?.Shutdown();
            _loader?.LogWriter?.Flush();
        }

        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            var message = $"Unhandled Exception: {exception.Message}\n" +
                          $"Stack Trace: {exception.StackTrace}";

            _loader?.Logger?.WriteLine(AddLogPrefix(message), _loader.Logger.ColorRed);
        }

        /* Utility Functions */

        /// <summary>
        /// Executes a given function and prints running stats of how long it took to execute.
        /// </summary>
        public static void ExecuteTimed(string text, Action action)
        {
            long initialTime = _stopWatch.ElapsedMilliseconds;
            action();
            _loader?.Logger?.WriteLineAsync(AddLogPrefix($"{text} | Time: {_stopWatch.ElapsedMilliseconds - initialTime}ms"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void HandleException(Exception ex)
        {
            // This method is singled out to avoid loading System.Windows.Forms at startup; because it is lazy loaded.
            var errorMessage = $"Failed to Load Reloaded-II.\n{ex.Message}\n{ex.StackTrace}\nA log is available at: {_loader?.LogWriter?.FlushPath}";
            _loader?.Console?.WaitForConsoleInit();
            _loader?.Logger?.WriteLine(errorMessage, _loader.Logger.ColorRed);
            _loader?.LogWriter?.Flush();
            User32.MessageBox(0, errorMessage, "Oh Noes!", 0);
        }

        private static void EnableProfileOptimization()
        {
            // Start Profile Optimization
            var profileRoot = Paths.ProfileOptimizationPath;
            Directory.CreateDirectory(profileRoot);
            
            // Define the folder where to save the profile files
            ProfileOptimization.SetProfileRoot(profileRoot);

            // Start profiling and save it in Startup.profile
            ProfileOptimization.StartProfile("Loader.profile");
        }
    }

    /// <summary>
    /// Parameters used to pass properties from native code to managed code.
    /// See: EntryPointParameters.h
    /// </summary>
    public struct EntryPointParameters
    {
        /*
            NOTE: Change CurrentVersion when adding new fields or extending current fields.
                  Also update EntryPointParameter.h in bootstrapper accordingly.
        */

        /// <summary>
        /// Current version of parameters.
        /// </summary>
        public const int CurrentVersion = 1;

        // Version 1
        public int Version;
        public EntryPointFlags Flags;

        // Version 2
        // ...

        /// <summary>
        /// Checks if struct is using latest version.
        /// </summary>
        public bool IsLatestVersion() => Version == CurrentVersion;

        /// <summary>
        /// Copies data from the passed in native struct to a new struct.
        /// This allows for old bootstrappers to work with more recent parameters.
        /// </summary>
        /// <param name="pointer">Pointer passed in from native code.</param>
        public static unsafe EntryPointParameters Copy(EntryPointParameters* pointer)
        {
            // Copy whole if possible.
            if (pointer->IsLatestVersion())
                return *pointer;

            // Otherwise construct from available size.
            EntryPointParameters result = default;

            // Version 1
            if (pointer->Version >= 1)
            {
                result.Version = pointer->Version;
                result.Flags = pointer->Flags;
            }

            return result;
        }
    }

    /// <summary>
    /// See: EntryPointParameters.h
    /// </summary>
    [Flags]
    public enum EntryPointFlags : int
    {
        None = 0,
        LoadedExternally = 1, 
    }

    // ReSharper restore UnusedMember.Global
}
