using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Reloaded.Hooks;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Utility.Parsers;
using Reloaded.Mod.Loader.Logging;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Utilities;
using Reloaded.Mod.Loader.Utilities.Native;
using Reloaded.Mod.Loader.Utilities.Steam;
using Reloaded.Mod.Shared;
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
                Logger?.LogWriteLineAsync($"Total Loader Initialization Time: {_stopWatch.ElapsedMilliseconds}ms");
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
                    Logger?.LogWriteLineAsync($"Bootstrapper (Reloaded.Mod.Loader.Bootstrapper.dll) is does not match expected version (Expected Version: {EntryPointParameters.CurrentVersion}, Actual Version: {parameters->Version}). Please upgrade the bootstrapper. If you are using ASI Loader re-deploy, otherwise copy Reloaded.Mod.Loader.Bootstrapper.dll.", Logger.ColorWarning);

                _parameters = EntryPointParameters.Copy(parameters);
            }
            else
            {
                Logger?.LogWriteLineAsync($"Expected EntryPointParameters but did not receive any. Bootstrapper (Reloaded.Mod.Loader.Bootstrapper.dll) is likely outdated. Please upgrade by copying a newer version of Reloaded.Mod.Loader.Bootstrapper.dll if integrating with another mod loader or re-deploy ASI Loader (if using ASI Loader).", Logger.ColorWarning);
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
                Logger?.LogWriteLineAsync($"Note: Reloaded is being loaded from an external source or mod loader.", Logger.ColorInformation);

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
                
                _delayInjector = new DelayInjector(() =>
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
        private static void HandleException(Exception ex)
        {
            // This method is singled out to avoid loading System.Windows.Forms at startup; because it is lazy loaded.
            var errorMessage = $"Failed to Load Reloaded-II.\n{ex.Message}\n{ex.StackTrace}\nA log is available at: {_loader?.LogWriter?.FlushPath}";
            _loader?.Console?.WaitForConsoleInit();
            Logger?.LogWriteLine(errorMessage, Logger.ColorError);
            _loader?.LogWriter?.Flush();
            User32.MessageBox(0, errorMessage, "Oh Noes!", 0);
        }
    }

    // ReSharper restore UnusedMember.Global
}
