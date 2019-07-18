using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// The <see cref="ApplicationLauncher"/> is a simple abstraction class that allows you to easily launch an application and Inject Reloaded II.
    /// </summary>
    public class ApplicationLauncher
    {
        private string _location;
        private string _commandline;

        private ApplicationLauncher() { }

        /// <summary>
        /// Creates an <see cref="ApplicationLauncher"/> given a full path to an executable.
        /// </summary>
        public ApplicationLauncher FromLocation(string location)
        {
            var launcher = new ApplicationLauncher();
            launcher._location = Path.GetFullPath(location);
            return launcher;
        }

        /// <summary>
        /// Creates an <see cref="ApplicationLauncher"/> from a whole commandline, i.e. path and arguments.
        /// </summary>
        public ApplicationLauncher FromCommandline(string commandline)
        {
            var launcher = new ApplicationLauncher();
            launcher._commandline = commandline;
            return launcher;
        }

        /// <summary>
        /// Starts the application, injecting Reloaded into it.
        /// </summary>
        public void Start()
        {
            // Start up the process
            Native.STARTUPINFO startupInfo                  = new Native.STARTUPINFO();
            Native.SECURITY_ATTRIBUTES lpProcessAttributes  = new Native.SECURITY_ATTRIBUTES();
            Native.SECURITY_ATTRIBUTES lpThreadAttributes   = new Native.SECURITY_ATTRIBUTES();
            Native.PROCESS_INFORMATION processInformation   = new Native.PROCESS_INFORMATION();

            bool success = false;

            if (!String.IsNullOrEmpty(_commandline))
            {
                var pathToExecutable = _commandline.Split(' ')[0];
                success = Native.CreateProcessW(null, _commandline, ref lpProcessAttributes,
                                                ref lpThreadAttributes, false, Native.ProcessCreationFlags.CREATE_SUSPENDED,
                                                IntPtr.Zero, Path.GetDirectoryName(pathToExecutable), ref startupInfo, ref processInformation);
            }
            else if (!String.IsNullOrEmpty(_location))
            {
                success = Native.CreateProcessW(null, _commandline, ref lpProcessAttributes,
                                                ref lpThreadAttributes, false, Native.ProcessCreationFlags.CREATE_SUSPENDED,
                                                IntPtr.Zero, Path.GetDirectoryName(_location), ref startupInfo, ref processInformation);
            }

            if (!success)
                throw new ArgumentException(Errors.FailedToStartProcess());

            // TODO: DLL Injection

        }

        #region Native Imports
        private class Native
        {
            [DllImport("kernel32.dll")]
            public static extern bool CreateProcessW([MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName, [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
                ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, ProcessCreationFlags dwCreationFlags, IntPtr lpEnvironment,
                [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
                ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

            [StructLayout(LayoutKind.Sequential)]
            public struct SECURITY_ATTRIBUTES
            {
                public int nLength;
                public IntPtr lpSecurityDescriptor;
                public int bInheritHandle;
            }

            public struct STARTUPINFO
            {
                public uint cb;
                public string lpReserved;
                public string lpDesktop;
                public string lpTitle;
                public uint dwX;
                public uint dwY;
                public uint dwXSize;
                public uint dwYSize;
                public uint dwXCountChars;
                public uint dwYCountChars;
                public uint dwFillAttribute;
                public uint dwFlags;
                public short wShowWindow;
                public short cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }


            public struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public uint dwProcessId;
                public uint dwThreadId;
            }

            [Flags]
            public enum ProcessCreationFlags : int
            {
                ZERO_FLAG = 0x00000000,
                CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
                CREATE_DEFAULT_ERROR_MODE = 0x04000000,
                CREATE_NEW_CONSOLE = 0x00000010,
                CREATE_NEW_PROCESS_GROUP = 0x00000200,
                CREATE_NO_WINDOW = 0x08000000,
                CREATE_PROTECTED_PROCESS = 0x00040000,
                CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
                CREATE_SEPARATE_WOW_VDM = 0x00001000,
                CREATE_SHARED_WOW_VDM = 0x00001000,
                CREATE_SUSPENDED = 0x00000004,
                CREATE_UNICODE_ENVIRONMENT = 0x00000400,
                DEBUG_ONLY_THIS_PROCESS = 0x00000002,
                DEBUG_PROCESS = 0x00000001,
                DETACHED_PROCESS = 0x00000008,
                EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
                INHERIT_PARENT_AFFINITY = 0x00010000
            }
        }
        #endregion Native Imports
    }
}
