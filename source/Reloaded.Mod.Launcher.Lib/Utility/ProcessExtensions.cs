namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Provides various extensions for interacting with Windows processes.
/// </summary>
public static unsafe class ProcessExtensions
{
    /* Constants */
    private const int ProcessQueryLimitedInformation = 0x1000;
    private const int MaxPath = 32767;
    private const string DefaultExecutablePath = "Unknown Path";

    /* Thread safety. */
    private static readonly object GetExecutablePathLock = new object();
    private static readonly object GetProcessIdLock = new object();
    private static int[] _processes = new int[1000];

    /* Buffer for communication. */
    private static readonly StringBuilder Buffer = new StringBuilder(MaxPath);

    /// <summary>
    /// Gets the executable path of a specified process.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <returns>The executable path for the process.</returns>
    public static string GetExecutablePath(this Process process) => GetExecutablePath(process.Id);

    /// <summary>
    /// Gets the executable path of a specified process.
    /// </summary>
    /// <param name="processId">The process id.</param>
    /// <returns>The executable path for the process.</returns>
    public static string GetExecutablePath(int processId)
    {
        // Vista+ implementation that is faster and allows to query system processes.
        var processHandle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
        if (processHandle == IntPtr.Zero)
            return DefaultExecutablePath;

        // Note: We can re-use the buffer without clearing because the returned string is null-terminated.
        lock (GetExecutablePathLock)
        {
            try
            {
                // ReSharper disable once NotAccessedVariable
                int size = Buffer.Capacity;
                if (QueryFullProcessImageNameW(processHandle, 0, Buffer, out size))
                    return Buffer.ToString();
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        return DefaultExecutablePath;
    }

    /// <summary>
    /// Returns the IDs of all processes on the system.
    /// </summary>
    /// <returns>IDs of all the processes.</returns>
    public static int[] GetProcessIds()
    {
        lock (GetProcessIdLock)
        {
            return GetProcessIdsInternal();
        }
    }

    private static int[] GetProcessIdsInternal()
    {
        // Get the list of process identifiers.
        int sizeOfProcesses = _processes.Length * sizeof(int);
        int bytesReturned;

        fixed (int* firstElement = _processes)
        {
            if (!EnumProcesses(firstElement, sizeOfProcesses, out bytesReturned))
                return Array.Empty<int>();
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
        System.Buffer.BlockCopy(_processes, 0, process, 0, bytesReturned);
        return process;
    }

    /* Open browser page */

    /// <summary>
    /// Opens a URL using Windows Explorer.
    /// </summary>
    public static void OpenFileWithExplorer(string url)
    {
        Process.Start(new ProcessStartInfo("cmd", $"/c start explorer \"{url}\"")
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }

    /// <summary>
    /// Opens an URL in browser.
    /// </summary>
    public static void OpenHyperlink(string url)
    {
        // Some links without full http(s) prefix might start with
        // / after markdown conversion. Need to fix those.
        url = url.TrimStart('/');
        if (!url.StartsWith("http") || !url.StartsWith("https"))
            url = $"http://{url}";

        OpenFileWithDefaultProgram(url);
    }

    /// <summary>
    /// Opens an URL with default associated program.
    /// </summary>
    public static void OpenFileWithDefaultProgram(string url)
    {
        Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{url}\"")
        {
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }

    /// <summary>
    /// Checks if a process is 64-bit or not.
    /// </summary>
    /// <param name="process">The process to check.</param>
    /// <returns>The process in question.</returns>
    public static bool Is64Bit(this Process process)
    {
        if (IntPtr.Size == 4)
            return false;

        return !(IsWow64Process(process.Handle, out bool isGame32Bit) && isGame32Bit);
    }

    [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

    /* Definitions */
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageNameW(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int size);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hHandle);

    [DllImport("Psapi.dll", SetLastError = true)]
    private static extern bool EnumProcesses(int* processIds, Int32 arraySizeBytes, out Int32 bytesCopied);
}