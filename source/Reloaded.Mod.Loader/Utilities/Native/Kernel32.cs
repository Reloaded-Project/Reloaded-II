using Microsoft.Win32.SafeHandles;

// ReSharper disable InconsistentNaming
namespace Reloaded.Mod.Loader.Utilities.Native;

public static class Kernel32
{
    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool add);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryW(string lpFileName);

    // Delegate type to be used as the Handler Routine for SCCH
    public delegate Boolean ConsoleCtrlDelegate(CtrlTypes CtrlType);

    // Enumerated type for the control messages sent to the handler routine
    public enum CtrlTypes : uint
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }

    #region Win32 SetUnhandledExceptionFilter
    /// <summary>
    /// Contains an exception record with a machine-independent description of an exception and a context record with a machine-dependent
    /// description of the processor context at the time of the exception.
    /// </summary>
    // typedef struct _EXCEPTION_POINTERS { PEXCEPTION_RECORD ExceptionRecord; PCONTEXT ContextRecord;} EXCEPTION_POINTERS, *PEXCEPTION_POINTERS;
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EXCEPTION_POINTERS
    {
        /// <summary>A pointer to an <c>EXCEPTION_RECORD</c> structure that contains a machine-independent description of the exception.</summary>
        public EXCEPTION_RECORD* ExceptionRecord;

        /// <summary>
        /// A pointer to a <c>CONTEXT</c> structure that contains a processor-specific description of the state of the processor at the
        /// time of the exception.
        /// </summary>
        public IntPtr ContextRecord;
    }

    /// <summary>
    /// An application-defined function that passes unhandled exceptions to the debugger, if the process is being debugged. Otherwise, it
    /// optionally displays an Application Error message box and causes the exception handler to be executed. This function can be called
    /// only from within the filter expression of an exception handler.
    /// </summary>
    /// <param name="exceptionInfo">
    /// A pointer to an EXCEPTION_POINTERS structure that specifies a description of the exception and the processor context at the time
    /// of the exception. This pointer is the return value of a call to the GetExceptionInformation function.
    /// </param>
    /// <returns>
    /// The function returns one of the following values.
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <term>Meaning</term>
    /// </listheader>
    /// <item>
    /// <term>EXCEPTION_CONTINUE_SEARCH = 0x0</term>
    /// <term>The process is being debugged, so the exception should be passed (as second chance) to the application's debugger.</term>
    /// </item>
    /// <item>
    /// <term>EXCEPTION_EXECUTE_HANDLER = 0x1</term>
    /// <term>
    /// If the SEM_NOGPFAULTERRORBOX flag was specified in a previous call to SetErrorMode, no Application Error message box is
    /// displayed. The function returns control to the exception handler, which is free to take any appropriate action.
    /// </term>
    /// </item>
    /// </list>
    /// </returns>
    public unsafe delegate EXCEPTION_FLAG PTOP_LEVEL_EXCEPTION_FILTER(EXCEPTION_POINTERS* exceptionInfo);

    /// <summary>Exception flags</summary>
    public enum EXCEPTION_FLAG : uint
    {
        /// <summary>Indicates a continuable exception.</summary>
        EXCEPTION_CONTINUABLE = 0,

        /// <summary>
        /// Proceed with normal execution of UnhandledExceptionFilter. That means obeying the SetErrorMode flags, or invoking the
        /// Application Error pop-up message box.
        /// </summary>
        EXCEPTION_CONTINUE_SEARCH = 0x0000,

        /// <summary>Indicates a noncontinuable exception.</summary>
        EXCEPTION_NONCONTINUABLE = 0x0001,

        /// <summary>
        /// Return from UnhandledExceptionFilter and execute the associated exception handler. This usually results in process termination.
        /// </summary>
        EXCEPTION_EXECUTE_HANDLER = 0x0001,

        /// <summary/>
        EXCEPTION_UNWINDING = 0x0002,

        /// <summary/>
        EXCEPTION_EXIT_UNWIND = 0x0004,

        /// <summary/>
        EXCEPTION_STACK_INVALID = 0x0008,

        /// <summary/>
        EXCEPTION_NESTED_CALL = 0x0010,

        /// <summary/>
        EXCEPTION_TARGET_UNWIND = 0x0020,

        /// <summary/>
        EXCEPTION_COLLIDED_UNWIND = 0x0040,

        /// <summary/>
        EXCEPTION_UNWIND = 0x0066,

        /// <summary>
        /// Return from UnhandledExceptionFilter and continue execution from the point of the exception. Note that the filter function is
        /// free to modify the continuation state by modifying the exception information supplied through its LPEXCEPTION_POINTERS parameter.
        /// </summary>
        EXCEPTION_CONTINUE_EXECUTION = 0xFFFFFFFF,

        /// <summary/>
        EXCEPTION_CHAIN_END = 0xFFFFFFFF,
    }
    #endregion

    #region MiniDumpWriteDump

    /// <summary>
    /// Writes user-mode minidump information to the specified file. https://msdn.microsoft.com/en-us/library/windows/desktop/ms680360.aspx
    /// </summary>
    /// <param name="hProcess">A handle to the process for which the information is to be generated.</param>
    /// <param name="processId">The identifier of the process for which the information is to be generated.</param>
    /// <param name="hFile">A handle to the file in which the information is to be written.</param>
    /// <param name="dumpType">The type of information to be generated. This parameter can be one or more of the values from the MINIDUMP_TYPE enumeration.</param>
    /// <param name="exceptionParam">A pointer to a MINIDUMP_EXCEPTION_INFORMATION structure describing the client exception that caused the minidump to be generated. If the value of this parameter is NULL, no exception information is included in the minidump file.</param>
    /// <param name="userStreamParam">A pointer to a MINIDUMP_USER_STREAM_INFORMATION structure. If the value of this parameter is NULL, no user-defined information is included in the minidump file.</param>
    /// <param name="callbackParam">A pointer to a MINIDUMP_CALLBACK_INFORMATION structure that specifies a callback routine which is to receive extended minidump information. If the value of this parameter is NULL, no callbacks are performed.</param>
    /// <returns>If the function succeeds, the return value is TRUE; otherwise, the return value is FALSE. To retrieve extended error information, call GetLastError. Note that the last error will be an HRESULT value. If the operation is canceled, the last error code is HRESULT_FROM_WIN32(ERROR_CANCELLED).</returns>
    [DllImport("Dbghelp.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeFileHandle hFile, MinidumpType dumpType, 
        ref MinidumpExceptionInformation exceptionParam, IntPtr userStreamParam, IntPtr callbackParam);

    /// <summary>
    /// Contains the exception information written to the minidump file by the MiniDumpWriteDump function.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct MinidumpExceptionInformation
    {
        /// <summary>
        /// The identifier of the thread throwing the exception.
        /// </summary>
        internal UInt32 ThreadId;

        /// <summary>
        /// A pointer to an EXCEPTION_POINTERS structure specifying a computer-independent description of the exception and the processor context at the time of the exception.
        /// </summary>
        internal IntPtr ExceptionPointers;

        /// <summary>
        /// Determines where to get the memory regions pointed to by the ExceptionPointers member. Set to TRUE if the memory resides in the process being debugged (the target process of the debugger). Otherwise, set to FALSE if the memory resides in the address space of the calling program (the debugger process). If you are accessing local memory (in the calling process) you should not set this member to TRUE.
        /// </summary>
        internal Boolean ClientPointers;
    }

    /// <summary>
    /// Identifies the type of information that will be written to the minidump file by the MiniDumpWriteDump function. https://msdn.microsoft.com/en-us/library/windows/desktop/ms680519.aspx
    /// </summary>
    [Flags]
    internal enum MinidumpType
    {
        /// <summary>
        /// Include just the information necessary to capture stack traces for all existing threads in a process.
        /// </summary>
        Normal = 0x00000000,

        /// <summary>
        /// Include the data sections from all loaded modules. This results in the inclusion of global variables, which can make the minidump file significantly larger. For per-module control, use the ModuleWriteDataSeg enumeration value from MODULE_WRITE_FLAGS.
        /// </summary>
        WithDataSegs = 0x00000001,

        /// <summary>
        /// Include all accessible memory in the process. The raw memory data is included at the end, so that the initial structures can be mapped directly without the raw memory information. This option can result in a very large file.
        /// </summary>
        WithFullMemory = 0x00000002,

        /// <summary>
        /// Include high-level information about the operating system handles that are active when the minidump is made.
        /// </summary>
        WithHandleData = 0x00000004,

        /// <summary>
        /// Stack and backing store memory written to the minidump file should be filtered to remove all but the pointer values necessary to reconstruct a stack trace.
        /// </summary>
        FilterMemory = 0x00000008,

        /// <summary>
        /// Stack and backing store memory should be scanned for pointer references to modules in the module list. If a module is referenced by stack or backing store memory, the ModuleWriteFlags member of the MINIDUMP_CALLBACK_OUTPUT structure is set to ModuleReferencedByMemory.
        /// </summary>
        ScanMemory = 0x00000010,

        /// <summary>
        /// Include information from the list of modules that were recently unloaded, if this information is maintained by the operating system.
        /// </summary>
        WithUnloadedModules = 0x00000020,

        /// <summary>
        /// Include pages with data referenced by locals or other stack memory. This option can increase the size of the minidump file significantly.
        /// </summary>
        WithIndirectlyReferencedMemory = 0x00000040,

        /// <summary>
        /// Filter module paths for information such as user names or important directories. This option may prevent the system from locating the image file and should be used only in special situations.
        /// </summary>
        FilterModulePaths = 0x00000080,

        /// <summary>
        /// Include complete per-process and per-thread information from the operating system.
        /// </summary>
        WithProcessThreadData = 0x00000100,

        /// <summary>
        /// Scan the virtual address space for PAGE_READWRITE memory to be included.
        /// </summary>
        WithPrivateReadWriteMemory = 0x00000200,

        /// <summary>
        /// Reduce the data that is dumped by eliminating memory regions that are not essential to meet criteria specified for the dump. This can avoid dumping memory that may contain data that is private to the user. However, it is not a guarantee that no private information will be present.
        /// </summary>
        WithoutOptionalData = 0x00000400,

        /// <summary>
        /// Include memory region information. For more information, see MINIDUMP_MEMORY_INFO_LIST.
        /// </summary>
        WithFullMemoryInfo = 0x00000800,

        /// <summary>
        /// Include thread state information. For more information, see MINIDUMP_THREAD_INFO_LIST.
        /// </summary>
        WithThreadInfo = 0x00001000,

        /// <summary>
        /// Include all code and code-related sections from loaded modules to capture executable content. For per-module control, use the ModuleWriteCodeSegs enumeration value from MODULE_WRITE_FLAGS.
        /// </summary>
        WithCodeSegs = 0x00002000,

        /// <summary>
        /// Turns off secondary auxiliary-supported memory gathering.
        /// </summary>
        WithoutAuxiliaryState = 0x00004000,

        /// <summary>
        /// Requests that auxiliary data providers include their state in the dump image; the state data that is included is provider dependent. This option can result in a large dump image.
        /// </summary>
        WithFullAuxiliaryState = 0x00008000,

        /// <summary>
        /// Scans the virtual address space for PAGE_WRITECOPY memory to be included.
        /// </summary>
        WithPrivateWriteCopyMemory = 0x00010000,

        /// <summary>
        /// If you specify MiniDumpWithFullMemory, the MiniDumpWriteDump function will fail if the function cannot read the memory regions; however, if you include MiniDumpIgnoreInaccessibleMemory, the MiniDumpWriteDump function will ignore the memory read failures and continue to generate the dump. Note that the inaccessible memory regions are not included in the dump.
        /// </summary>
        IgnoreInaccessibleMemory = 0x00020000,

        /// <summary>
        /// Adds security token related data. This will make the "!token" extension work when processing a user-mode dump.
        /// </summary>
        WithTokenInformation = 0x00040000,

        /// <summary>
        /// Adds module header related data.
        /// </summary>
        WithModuleHeaders = 0x00080000,

        /// <summary>
        /// Adds filter triage related data.
        /// </summary>
        FilterTriage = 0x00100000,

        /// <summary>
        /// Indicates which flags are valid.
        /// </summary>
        ValidTypeFlags = 0x001fffff
    }

    /// <summary>Describes an exception.</summary>
    // typedef struct _EXCEPTION_RECORD { DWORD ExceptionCode; DWORD ExceptionFlags; struct _EXCEPTION_RECORD *ExceptionRecord; PVOID
    // ExceptionAddress; DWORD NumberParameters; ULONG_PTR ExceptionInformation[EXCEPTION_MAXIMUM_PARAMETERS];} EXCEPTION_RECORD, *PEXCEPTION_RECORD;
    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_RECORD
    {
        /// <summary>
        /// The exception flags. This member can be either zero, indicating a continuable exception, or <c>EXCEPTION_NONCONTINUABLE</c>
        /// indicating a noncontinuable exception. Any attempt to continue execution after a noncontinuable exception causes the
        /// <c>EXCEPTION_NONCONTINUABLE_EXCEPTION</c> exception.
        /// </summary>
        public ExceptionCode ExceptionCode;

        /// <summary>
        /// The exception flags. This member can be either zero, indicating a continuable exception, or <c>EXCEPTION_NONCONTINUABLE</c>
        /// indicating a noncontinuable exception. Any attempt to continue execution after a noncontinuable exception causes the
        /// <c>EXCEPTION_NONCONTINUABLE_EXCEPTION</c> exception.
        /// </summary>
        public EXCEPTION_FLAG ExceptionFlags;

        /// <summary>
        /// A pointer to an associated <c>EXCEPTION_RECORD</c> structure. Exception records can be chained together to provide additional
        /// information when nested exceptions occur.
        /// </summary>
        public unsafe EXCEPTION_RECORD* ExceptionRecord;

        /// <summary>The address where the exception occurred.</summary>
        public UIntPtr ExceptionAddress;

        /// <summary>
        /// The number of parameters associated with the exception. This is the number of defined elements in the
        /// <c>ExceptionInformation</c> array.
        /// </summary>
        public uint NumberParameters;
    }

    /// <summary>Common Exception codes</summary>
    /// <remarks>
    /// Users can define their own exception codes, so the code could be any value. The OS reserves bit 28 and may clear that for its own purposes
    /// </remarks>
    public enum ExceptionCode : uint
    {
        /// <summary/>
        None = 0x0,

        /// <summary/>
        STATUS_BREAKPOINT = 0x80000003,

        /// <summary/>
        STATUS_SINGLESTEP = 0x80000004,

        /// <summary/>
        EXCEPTION_INT_DIVIDE_BY_ZERO = 0xC0000094,

        /// <summary>Fired when debuggee gets a Control-C.</summary>
        DBG_CONTROL_C = 0x40010005,

        /// <summary/>
        EXCEPTION_STACK_OVERFLOW = 0xC00000FD,

        /// <summary/>
        EXCEPTION_NONCONTINUABLE_EXCEPTION = 0xC0000025,

        /// <summary/>
        EXCEPTION_ACCESS_VIOLATION = 0xc0000005,
    }
    #endregion

    /// <summary>
    /// Gets the ID of the currently executing thread.
    /// </summary>
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThread();
}