using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Utilities;

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

        /* Ensures DLL Resolution */
        public static void Main() { } // Dummy for R2R images.
        private static void SetupLoader()
        {
            try
            {
                // Setup mod loader.
                _stopWatch = new Stopwatch();
                _stopWatch.Start();

                ExecuteTimed("Create Loader", CreateLoader);
                var createHostTask = Task.Run(() => ExecuteTimed("Create Loader Host (Async)", CreateHost));
                var checkDrmTask   = Task.Run(() => ExecuteTimed("Checking for DRM (Async)", CheckForDRM));
                ExecuteTimed("Loading Mods", LoadMods);

                checkDrmTask.Wait();
                createHostTask.Wait();
                Console.WriteLine($"[Reloaded] Total Loader Initialization Time: {_stopWatch.ElapsedMilliseconds}ms");
                _stopWatch.Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Load Reloaded-II.\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void LoadMods() => _loader.LoadForCurrentProcess();
        private static void CheckForDRM() => DRMNotifier.PrintWarnings(_loader.Console);
        private static void CreateLoader() => _loader = new Loader();
        private static void CreateHost() => _server = new Host(_loader);

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Initializes the mod loader.
        /// Returns the port on the local machine (but that wouldn't probably be used).
        /// </summary>
        public static int Initialize(IntPtr unusedPtr, int unusedSize)
        {
            // Write port as a Memory Mapped File, to allow Mod Loader's Launcher to discover the mod port.
            // (And to stop bootstrapper from loading loader again).
            int pid             = Process.GetCurrentProcess().Id;
            _memoryMappedFile   = MemoryMappedFile.CreateOrOpen(ServerUtility.GetMappedFileNameForPid(pid), sizeof(int));
            var view            = _memoryMappedFile.CreateViewStream();
            var binaryWriter    = new BinaryWriter(view);
            binaryWriter.Write((int)0);

            // Setup Loader
            SetupLoader();

            // Only write port on completed initialization.
            // If port is 0, assume in loading state
            binaryWriter.Seek(-sizeof(int), SeekOrigin.Current);
            binaryWriter.Write(_server.Port);
            return _server?.Port ?? 0;
        }

        /* Utility Functions */

        /// <summary>
        /// Executes a given function and prints running stats of how long it took to execute.
        /// </summary>
        public static void ExecuteTimed(string text, Action action)
        {
            long initialTime = _stopWatch.ElapsedMilliseconds;
            action();
            Console.WriteLine($"[Reloaded | Benchmark] {text} | Time: {_stopWatch.ElapsedMilliseconds - initialTime}ms");
        }
    }
    // ReSharper restore UnusedMember.Global
}
