using System;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Loader.Bootstrap;

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
        /// Returns the port on the local machine
        /// </summary>
        public static int GetPort()
        {
            return _server.Port;
        }
    }
    // ReSharper restore UnusedMember.Global
}
