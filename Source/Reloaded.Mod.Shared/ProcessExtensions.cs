using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Reloaded.Mod.Shared
{
    public static class ProcessExtensions
    {
        /* Constants */
        public const int ProcessQueryLimitedInformation = 0x1000;
        public const int MaxPath = 32767;

        /* Thread safety. */
        private static object _lock = new object();
        private static int[] _processes = new int[1000];

        /* Buffer for communication. */
        private static readonly StringBuilder _buffer = new StringBuilder(MaxPath);

        public static string GetExecutablePath(this Process process) => GetExecutablePath(process.Id);
        public static string GetExecutablePath(int processId)
        {
            /* Vista+ implementation that is faster and allows to query system processes. */
            var processHandle = OpenProcess(ProcessQueryLimitedInformation, false, processId);

            // Note: We can re-use the buffer without clearing because the returned string is null-terminated.

            lock (_lock)
            {
                if (processHandle != IntPtr.Zero)
                {
                    try
                    {
                        // ReSharper disable once NotAccessedVariable
                        int size = _buffer.Capacity;
                        if (QueryFullProcessImageNameW(processHandle, 0, _buffer, out size))
                            return _buffer.ToString();
                    }
                    finally
                    {
                        CloseHandle(processHandle);
                    }
                }
            }

            return "Unknown Path";
        }

        public static unsafe int[] GetProcessIds()
        {
            // Get the list of process identifiers.
            int sizeOfProcesses = _processes.Length * sizeof(int);
            int bytesReturned;

            fixed (int* firstElement = _processes)
            {
                if (!EnumProcesses(firstElement, sizeOfProcesses, out bytesReturned))
                    return new int[0];
            }

            // Print the name and process identifier for each process.
            if (sizeOfProcesses <= bytesReturned)
            {
                _processes = new int[_processes.Length * 2];
                return GetProcessIds();
            }

            // Calculate how many process identifiers were returned.
            int processNumber = bytesReturned / sizeof(uint);
            int[] process = new int[processNumber];
            Buffer.BlockCopy(_processes, 0, process, 0, processNumber);
            return process;
        }

        /* Open browser page */
        public static void OpenFileWithExplorer(string url)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{url}\""));
        }

        public static void OpenFileWithDefaultProgram(string url)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
        }

        /* Definitions */

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool QueryFullProcessImageNameW(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("Psapi.dll", SetLastError = true)]
        public static extern unsafe bool EnumProcesses(int* processIds, Int32 arraySizeBytes, out Int32 bytesCopied);
    }
}
