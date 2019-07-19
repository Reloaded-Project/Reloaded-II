using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Loader.Bootstrap;
using Reloaded.Mod.Loader.Server;

namespace Reloaded.Mod.Loader
{
    /// <summary>
    /// Provides an entry point to this .NET Core application for the native C++ bootstrapper.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public static class EntryPoint
    {
        // DO NOT RENAME THIS CLASS OR ITS PUBLIC METHODS
        private static Loader _loader;
        private static Host _server;
        private static MemoryMappedFile _memoryMappedFile;

        /* Ensures DLL Resolution */
        static EntryPoint()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyResolver.ResolveAssembly;
                SetupLoader();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Load Reloaded-II.\n{ex.Message}");
            }
        }

        private static void SetupLoader()
        {
            // Setup mod loader.
            _loader = new Loader();
            _loader.LoadForCurrentProcess();

            _server = new Host(_loader);
        }

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Initializes the mod loader.
        /// Returns the port on the local machine (but that wouldn't probably be used).
        /// </summary>
        public static int Initialize(IntPtr unusedPtr, int unusedSize)
        {
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
