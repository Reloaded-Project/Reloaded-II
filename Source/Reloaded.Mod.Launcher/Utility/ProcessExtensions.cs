using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Reloaded.Mod.Launcher.Utility
{
    public static class ProcessExtensions
    {
        public static bool Is64Bit(this Process process)
        {
            if (IntPtr.Size == 4)
                return false;

            return !(IsWow64Process(process.Handle, out bool isGame32Bit) && isGame32Bit);
        }

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);
    }
}
