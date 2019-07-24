using System;
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
        public static object Lock = new object();

        /* Buffer for communication. */
        private static readonly StringBuilder _buffer = new StringBuilder(MaxPath);

        public static string GetExecutablePath(this Process process) => GetExecutablePath(process.Id);
        public static string GetExecutablePath(int processId)
        {
            /* Vista+ implementation that is faster and allows to query system processes. */
            var processHandle = OpenProcess(ProcessQueryLimitedInformation, false, processId);

            // Note: We can re-use the buffer without clearing because the returned string is null-terminated.

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

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /* Open browser page */
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
    }
}
