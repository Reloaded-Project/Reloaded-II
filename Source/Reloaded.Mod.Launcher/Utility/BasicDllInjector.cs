using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Provides the implementation of a very basic, primitive DLL Injector.
    /// This one is required to inject into an application in suspended state.
    ///
    /// WARNING: USE FROM 64-bit process only!
    /// </summary>
    public class BasicDllInjector
    {
        private static IntPtr x64LoadLibraryAddress;
        private static IntPtr x86LoadLibraryAddress;

        private IntPtr _loadLibraryAddress;

        private readonly Process _process;
        private readonly ExternalMemory _memory;

        static BasicDllInjector()
        {
            x64LoadLibraryAddress = Getx64LoadLibraryAddress();
            x86LoadLibraryAddress = Getx86LoadLibraryAddress();
        }

        /// <summary>
        /// Provides the implementation of a primitive DLL injector, supporting injection into
        /// suspended process.
        /// </summary>
        public BasicDllInjector(Process process)
        {
            // Set the Location and Handle to the Process to be Injected.
            _process = process;
            _memory = new ExternalMemory(process);

            _loadLibraryAddress = _process.Is64Bit() ? x64LoadLibraryAddress : x86LoadLibraryAddress;
        }

        /// <param name="libraryPath">Full path to library to load.</param>
        /// <returns>0 if injection failed.</returns>
        public int Inject(string libraryPath)
        {
            IntPtr libraryNameMemoryAddress = WriteLoadLibraryParameter(libraryPath);
            int result = ExecuteFunction(_loadLibraryAddress, libraryNameMemoryAddress);
            _memory.Free(libraryNameMemoryAddress);
            return result;
        }

        public static void PreloadAddresses()
        {
            // Dummy. Static constructor only needed.
        }

        private IntPtr WriteLoadLibraryParameter(string libraryPath)
        {
            byte[] libraryNameBytes = Encoding.Unicode.GetBytes(libraryPath);
            IntPtr processPointer   = _memory.Allocate(libraryNameBytes.Length);
            _memory.WriteRaw(processPointer, libraryNameBytes);
            return processPointer;
        }

        private int ExecuteFunction(IntPtr address, IntPtr parameterAddress)
        {
            IntPtr hThread = CreateRemoteThread(_process.Handle, IntPtr.Zero, IntPtr.Zero, address, parameterAddress, 0, out IntPtr threadId);
            WaitForSingleObject(hThread, unchecked((uint)-1));
            GetExitCodeThread(hThread, out uint exitCode);
            return (int)exitCode;
        }

        /* Helper functions */

        private static IntPtr Getx64LoadLibraryAddress()
        {
            var kernel32Handle = LoadLibraryW("kernel32");
            return GetProcAddress(kernel32Handle, "LoadLibraryW");
        }

        private static IntPtr Getx86LoadLibraryAddress()
        {
            // Setup Memory Mapped File for transfer.
            var file = MemoryMappedFile.CreateOrOpen(SharedConstants.Kernel32AddressDumperMemoryMappedFileName, sizeof(long));

            // Load dummy 32bit process to get 32bit addresses.
            var kernelDumpProcess  = StartKernelAddressDumper();
            kernelDumpProcess.WaitForExit();

            var viewStream = file.CreateViewStream();
            var reader = new BinaryReader(viewStream);
            var result = (IntPtr)reader.ReadInt64();

            if (result == IntPtr.Zero)
                throw new Exception("Failed to acquire address of GetProcAddress for x86.");

            return result;
        }

        private static Process StartKernelAddressDumper()
        {
            string location = AppDomain.CurrentDomain.BaseDirectory + $"\\{LoaderConfig.Kernel32AddressDumperName}";
            return Process.Start(location);
        }

        #region Native Imports
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, IntPtr dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        private static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
        #endregion
    }
}
