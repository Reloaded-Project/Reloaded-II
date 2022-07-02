using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Kernel32AddressDumper
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            nuint loadLibraryAddress  = GetLoadLibraryAddress();
            byte[] bytes              = BitConverter.GetBytes((long) loadLibraryAddress);

            var file = MemoryMappedFile.OpenExisting(SharedConstants.Kernel32AddressDumperMemoryMappedFileName);
            var viewStream = file.CreateViewStream();
            viewStream.Write(bytes, 0, bytes.Length);
        }

        private static nuint GetLoadLibraryAddress()
        {
            var kernel32Handle = LoadLibraryW("kernel32");
            return GetProcAddress(kernel32Handle, "LoadLibraryW");
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern nuint LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern nuint GetProcAddress(nuint hModule, string procName);
    }
}
