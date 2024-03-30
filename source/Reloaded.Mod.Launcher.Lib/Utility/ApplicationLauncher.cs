namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// The <see cref="ApplicationLauncher"/> is a simple abstraction class that allows you to easily launch an application and Inject Reloaded II.
/// </summary>
public class ApplicationLauncher
{
    private string _location = "";
    private string _arguments = "";
    private string _workingDirectory = "";

    /// <summary/>
    private ApplicationLauncher() { }

    /// <summary>
    /// Creates an <see cref="ApplicationLauncher"/> from a whole commandline, i.e. path and arguments.
    /// </summary>
    public static ApplicationLauncher FromLocationAndArguments(string location, string? arguments, string? workingDirectory)
    {
        var launcher = new ApplicationLauncher
        {
            _arguments = arguments ?? "",
            _location = location,
            _workingDirectory = string.IsNullOrEmpty(workingDirectory) ? Path.GetDirectoryName(location)! : workingDirectory
        };

        if (!File.Exists(launcher._location))
            throw new ArgumentException(Resources.ErrorPathToApplicationInvalid.Get());

        return launcher;
    }

    /// <summary>
    /// Creates an <see cref="ApplicationLauncher"/> from an application config.
    /// </summary>
    public static ApplicationLauncher FromApplicationConfig(PathTuple<ApplicationConfig> config)
    {
        return FromLocationAndArguments($"{ApplicationConfig.GetAbsoluteAppLocation(config)}", $"{config.Config.AppArguments}", config.Config.WorkingDirectory);
    }

    /// <summary>
    /// Starts the application, injecting Reloaded into it.
    /// </summary>
    public void Start(bool inject = true)
    {
        // Start up the process
        Native.STARTUPINFO startupInfo                  = new Native.STARTUPINFO();
        Native.SECURITY_ATTRIBUTES lpProcessAttributes  = new Native.SECURITY_ATTRIBUTES();
        Native.SECURITY_ATTRIBUTES lpThreadAttributes   = new Native.SECURITY_ATTRIBUTES();
        Native.PROCESS_INFORMATION processInformation   = new Native.PROCESS_INFORMATION();

        if (_arguments == null)
            _arguments = "";

        bool success = Native.CreateProcessW(null, $"\"{_location}\" {_arguments}", ref lpProcessAttributes,
            ref lpThreadAttributes, false, Native.ProcessCreationFlags.CREATE_SUSPENDED,
            IntPtr.Zero, _workingDirectory, ref startupInfo, ref processInformation);

        if (!success)
        {
            string windowsErrorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            throw new ArgumentException($"{Resources.ErrorFailedToStartProcess.Get()} {windowsErrorMessage}");
        }

        // DLL Injection
        var process         = Process.GetProcessById((int) processInformation.dwProcessId);
        using var injector  = new ApplicationInjector(process);

        if (inject)
        {
            try
            {
                injector.Inject();
            }
            catch (Exception)
            {
                Native.ResumeThread(processInformation.hThread);
                throw;
            }
        }

        Native.ResumeThread(processInformation.hThread);
    }

    #region Native Imports
    private class Native
    {
        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreateProcessW([MarshalAs(UnmanagedType.LPWStr)] string? lpApplicationName, [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
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