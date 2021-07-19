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
        private static IHook<ExitProcess> _exitProcessHook;
        private static DelayInjector _delayInjector;
        private static Process _process;
        private static SteamHook _steamHook;

        /* For ReadyToRun Feature */
        public static void Main() { } // Dummy for R2R images.

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Initializes the mod loader.
        /// Returns the port on the local machine (but that wouldn't probably be used).
        /// </summary>
        public static int Initialize(IntPtr unusedPtr, int unusedSize)
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
            SetupLoader();

            // Only write port on completed initialization.
            // If port is 0, assume in loading state
            binaryWriter.Seek(-sizeof(int), SeekOrigin.Current);
            binaryWriter.Write(_server.Port);
            return _server?.Port ?? 0;
        }

        private static void SetupLoader()
        {
            try
            {
                // Setup mod loader.
                _stopWatch = new Stopwatch();
                _stopWatch.Start();

                AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
                ExecuteTimed("Create Loader", CreateLoader);
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
        private static void CreateHost()   => _server = new Host(_loader);
        private static void SetupHooks()
        {
            // Hook native import for ExitProcess. (So we can save log on exit)
            var kernel32 = Kernel32.GetModuleHandle("kernel32.dll");
            var address = Kernel32.GetProcAddress(kernel32, "ExitProcess");
            if (address != IntPtr.Zero)
                _exitProcessHook = new Hook<ExitProcess>(ExitProcessImpl, (long)address).Activate();

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
            
            if (!requiresDelayStart)
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

        private static void ExitProcessImpl(uint uExitCode)
        {
            SaveAndFlushLog();
            _exitProcessHook.OriginalFunction(uExitCode);
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
        
        [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
        [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Cdecl)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ExitProcess(uint uExitCode);
    }
    // ReSharper restore UnusedMember.Global
}
