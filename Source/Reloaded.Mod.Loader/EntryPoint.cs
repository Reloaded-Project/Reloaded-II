using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Reloaded.Mod.Loader.Bootstrap;
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
        public static Stopwatch StopWatch { get; set; }
        private static Loader _loader;
        private static Host _server;
        private static MemoryMappedFile _memoryMappedFile;
        private static Task _setupServerTask;

        /* Ensures DLL Resolution */
        public static void Main() { } // Dummy for R2R images.
        static EntryPoint()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyResolver.ResolveAssembly;
                SetupLoader();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Load Reloaded-II.\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void SetupLoader()
        {
            void SetupServer()
            {
                try
                {
                    _server = new Host(_loader);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to start Reloaded-II mod loader server." + e.Message);
                    throw;
                }
            }

            // Setup mod loader.
            StopWatch = new Stopwatch();
            StopWatch.Start();
            _loader = new Loader();
            DRMScanner.PrintWarnings(_loader.Console); // Print preliminary warnings.
            _setupServerTask = Task.Run(SetupServer);
            _loader.LoadForCurrentProcess();

            // The reason the server setup is being done on an alternate thread is to reduce startup times.
            // The main reason is the reduction of JIT overhead in startup times, as the server code can be JIT-ted
            // while the mod loader is loading mods.

            // i.e. Server can be loaded fully in parallel to loader.
        }

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Initializes the mod loader.
        /// Returns the port on the local machine (but that wouldn't probably be used).
        /// </summary>
        public static int Initialize(IntPtr unusedPtr, int unusedSize)
        {
            // Wait for server to finish.
            _setupServerTask.Wait();

            // Write port as a Memory Mapped File, to allow Mod Loader's Launcher to discover the mod port.
            int pid             = Process.GetCurrentProcess().Id;
            _memoryMappedFile   = MemoryMappedFile.CreateOrOpen(ServerUtility.GetMappedFileNameForPid(pid), sizeof(int));
            var view            = _memoryMappedFile.CreateViewStream();
            var binaryWriter    = new BinaryWriter(view);
            binaryWriter.Write(_server.Port);

            return _server?.Port ?? 0;
        }
    }
    // ReSharper restore UnusedMember.Global
}
