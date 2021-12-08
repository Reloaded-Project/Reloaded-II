using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Provides the implementation of a very basic, primitive DLL Injector.
/// This one is required to inject into an application in suspended state.
///
/// WARNING: USE FROM 64-bit process only!
/// </summary>
public class BasicDllInjector
{
    private static IntPtr _x64LoadLibraryAddress;
    private static IntPtr _x86LoadLibraryAddress;
    private static bool _initialized;

    private readonly Process _process;
    private readonly ExternalMemory _memory;

    /// <summary>
    /// Provides the implementation of a primitive DLL injector, supporting injection into
    /// suspended process.
    /// </summary>
    public BasicDllInjector(Process process)
    {
        // Set the Location and Handle to the Process to be Injected.
        _process = process;
        _memory  = new ExternalMemory(process);
    }

    /// <param name="libraryPath">Full path to library to load.</param>
    /// <returns>0 if injection failed.</returns>
    public int Inject(string libraryPath)
    {
        if (!_initialized)
            PreloadAddresses();

        var loadLibraryAddress = _process.Is64Bit() ? _x64LoadLibraryAddress : _x86LoadLibraryAddress;
        IntPtr libraryNameMemoryAddress = WriteLoadLibraryParameter(libraryPath);
        int result = ExecuteFunction(loadLibraryAddress, libraryNameMemoryAddress);
        _memory.Free(libraryNameMemoryAddress);
        return result;
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
        IntPtr hThread = CreateRemoteThread(_process.Handle, IntPtr.Zero, IntPtr.Zero, address, parameterAddress, 0, out _);
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
            throw new Exception(Resources.ErrorGetProcAddress32Failed.Get());

        return result;
    }

    private static Process StartKernelAddressDumper()
    {
        string location = Paths.GetKernel32AddressDumperPath(AppDomain.CurrentDomain.BaseDirectory);
        return Process.Start(location);
    }

    private static void PreloadAddresses()
    {
        // Dummy. Static constructor only needed.
        if (_initialized)
            return;

        ActionWrappers.TryCatch(() => { _x64LoadLibraryAddress = Getx64LoadLibraryAddress(); });
        ActionWrappers.TryCatch(() => { _x86LoadLibraryAddress = Getx86LoadLibraryAddress(); });
        _initialized = true;
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