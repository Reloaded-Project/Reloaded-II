using System;
using System.Runtime.InteropServices;

namespace Reloaded.Mod.Loader.Logging.Init
{
    public static class ConsoleAllocator
    {
        /// <summary>
        /// Returns true if a console window is present for this process, else false.
        /// </summary>
        public static bool ConsoleExists => GetConsoleWindow() != IntPtr.Zero;

        /// <summary>
        /// Creates a new console instance if no console is present for the console.
        /// </summary>
        public static void Alloc()
        {
            if (!ConsoleExists)
                AllocConsole();
        }

        /// <summary>
        /// If the process has an associated console, it will be detached and no longer visible.
        /// </summary>
        public static void Free()
        {
            if (ConsoleExists)
                FreeConsole();
        }


        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
    }
}
