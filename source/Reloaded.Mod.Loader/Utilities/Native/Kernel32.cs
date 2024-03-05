using Microsoft.Win32.SafeHandles;

// ReSharper disable InconsistentNaming
namespace Reloaded.Mod.Loader.Utilities.Native;

public static partial class Kernel32
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
    
    /// <summary>
    /// Retrieves information about a range of pages in the virtual address space of the calling process.
    /// </summary>
    /// <param name="lpAddress">A pointer to the base address of the region of pages to be queried.</param>
    /// <param name="lpBuffer">A pointer to a buffer that receives the information. The buffer is a MEMORY_BASIC_INFORMATION structure.</param>
    /// <param name="dwLength">The size of the buffer pointed to by the lpBuffer parameter, in bytes.</param>
    /// <returns>If the function succeeds, the return value is the actual number of bytes returned. If the function fails, the return value is zero.</returns>
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static unsafe partial nuint VirtualQuery(nuint lpAddress, MEMORY_BASIC_INFORMATION* lpBuffer, nuint dwLength);
    
    /// <summary>
    /// <para>
    /// Contains information about a range of pages in the virtual address space of a process. The VirtualQuery and VirtualQueryEx
    /// functions use this structure.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// To enable a debugger to debug a target that is running on a different architecture (32-bit versus 64-bit), use one of the
    /// explicit forms of this structure.
    /// </para>
    /// </remarks>
    public struct MEMORY_BASIC_INFORMATION
    {
        /// <summary>
        /// <para>A pointer to the base address of the region of pages.</para>
        /// </summary>
        public nuint BaseAddress;

        /// <summary>
        /// <para>
        /// A pointer to the base address of a range of pages allocated by the VirtualAlloc function. The page pointed to by the
        /// <c>BaseAddress</c> member is contained within this allocation range.
        /// </para>
        /// </summary>
        public nuint AllocationBase;

        /// <summary>
        /// <para>
        /// The memory protection option when the region was initially allocated. This member can be one of the memory protection
        /// constants or 0 if the caller does not have access.
        /// </para>
        /// </summary>
        public MEM_PROTECTION AllocationProtect;

        /// <summary>
        /// <para>The size of the region beginning at the base address in which all pages have identical attributes, in bytes.</para>
        /// </summary>
        public nuint RegionSize;

        /// <summary>
        /// <para>The state of the pages in the region. This member can be one of the following values.</para>
        /// <list type="table">
        /// <listheader>
        /// <term>State</term>
        /// <term>Meaning</term>
        /// </listheader>
        /// <item>
        /// <term>MEM_COMMIT 0x1000</term>
        /// <term>
        /// Indicates committed pages for which physical storage has been allocated, either in memory or in the paging file on disk.
        /// </term>
        /// </item>
        /// <item>
        /// <term>MEM_FREE 0x10000</term>
        /// <term>
        /// Indicates free pages not accessible to the calling process and available to be allocated. For free pages, the information in
        /// the AllocationBase, AllocationProtect, Protect, and Type members is undefined.
        /// </term>
        /// </item>
        /// <item>
        /// <term>MEM_RESERVE 0x2000</term>
        /// <term>
        /// Indicates reserved pages where a range of the process's virtual address space is reserved without any physical storage being
        /// allocated. For reserved pages, the information in the Protect member is undefined.
        /// </term>
        /// </item>
        /// </list>
        /// </summary>
        public MEM_STATE State;

        /// <summary>
        /// <para>
        /// The access protection of the pages in the region. This member is one of the values listed for the <c>AllocationProtect</c> member.
        /// </para>
        /// </summary>
        public MEM_PROTECTION Protect;

        /// <summary>
        /// <para>The type of pages in the region. The following types are defined.</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Type</term>
        /// <term>Meaning</term>
        /// </listheader>
        /// <item>
        /// <term>MEM_IMAGE 0x1000000</term>
        /// <term>Indicates that the memory pages within the region are mapped into the view of an image section.</term>
        /// </item>
        /// <item>
        /// <term>MEM_MAPPED 0x40000</term>
        /// <term>Indicates that the memory pages within the region are mapped into the view of a section.</term>
        /// </item>
        /// <item>
        /// <term>MEM_PRIVATE 0x20000</term>
        /// <term>Indicates that the memory pages within the region are private (that is, not shared by other processes).</term>
        /// </item>
        /// </list>
        /// </summary>
        public MEM_TYPE Type;
    }

    /// <summary>
    /// Represents the state of the memory pages within the region.
    /// </summary>
    public enum MEM_STATE : uint
    {
        /// <summary>
        /// Indicates committed pages for which physical storage has been allocated, either in memory or in the paging file on disk.
        /// </summary>
        COMMIT = 0x1000,

        /// <summary>
        /// Indicates free pages not accessible to the calling process and available to be allocated.
        /// </summary>
        FREE = 0x10000,

        /// <summary>
        /// Indicates reserved pages where a range of the process's virtual address space is reserved without any physical storage being allocated.
        /// </summary>
        RESERVE = 0x2000
    }
    
    /// <summary>
    /// Represents the type of the memory pages within the region.
    /// </summary>
    public enum MEM_TYPE : uint
    {
        /// <summary>
        /// Indicates that the memory pages within the region are mapped into the view of an image section.
        /// </summary>
        IMAGE = 0x1000000,

        /// <summary>
        /// Indicates that the memory pages within the region are mapped into the view of a section.
        /// </summary>
        MAPPED = 0x40000,

        /// <summary>
        /// Indicates that the memory pages within the region are private (that is, not shared by other processes).
        /// </summary>
        PRIVATE = 0x20000
    }
    
    /// <summary>
    ///     The following are the memory-protection options; you must specify one of the following values when allocating or
    ///     protecting a page in memory.
    ///     Protection attributes cannot be assigned to a portion of a page; they can only be assigned to a whole page.
    /// </summary>
    [Flags]
    public enum MEM_PROTECTION : uint
    {
        /// <summary>
        ///     Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed
        ///     region results in an access violation.
        ///     <para>This flag is not supported by the CreateFileMapping function.</para>
        /// </summary>
        PAGE_NOACCESS = 1,

        /// <summary>
        ///     Enables read-only access to the committed region of pages. An attempt to write to the committed region results in
        ///     an access violation. If Data
        ///     Execution Prevention is enabled, an attempt to execute code in the committed region results in an access violation.
        /// </summary>
        PAGE_READONLY = 2,

        /// <summary>
        ///     Enables read-only or read/write access to the committed region of pages. If Data Execution Prevention is enabled,
        ///     attempting to execute code in
        ///     the committed region results in an access violation.
        /// </summary>
        PAGE_READWRITE = 4,

        /// <summary>
        ///     Enables read-only or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a
        ///     committed copy-on-write page
        ///     results in a private copy of the page being made for the process. The private page is marked as PAGE_READWRITE, and
        ///     the change is written to the
        ///     new page. If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an
        ///     access violation.
        ///     <para>This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.</para>
        /// </summary>
        PAGE_WRITECOPY = 8,

        /// <summary>
        ///     Enables execute access to the committed region of pages. An attempt to write to the committed region results in an
        ///     access violation.
        ///     <para>This flag is not supported by the CreateFileMapping function.</para>
        /// </summary>
        PAGE_EXECUTE = 16, // 0x00000010

        /// <summary>
        ///     Enables execute or read-only access to the committed region of pages. An attempt to write to the committed region
        ///     results in an access violation.
        ///     <para>
        ///         Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function until
        ///         Windows XP with SP2 and Windows
        ///         Server 2003 with SP1.
        ///     </para>
        /// </summary>
        PAGE_EXECUTE_READ = 32, // 0x00000020

        /// <summary>
        ///     Enables execute, read-only, or read/write access to the committed region of pages.
        ///     <para>
        ///         Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function until
        ///         Windows XP with SP2 and Windows
        ///         Server 2003 with SP1.
        ///     </para>
        /// </summary>
        PAGE_EXECUTE_READWRITE = 64, // 0x00000040

        /// <summary>
        ///     Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. An attempt to write
        ///     to a committed copy-on-write
        ///     page results in a private copy of the page being made for the process. The private page is marked as
        ///     PAGE_EXECUTE_READWRITE, and the change is
        ///     written to the new page.
        ///     <para>This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.</para>
        ///     <para>
        ///         Windows Vista, Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping
        ///         function until Windows Vista with SP1
        ///         and Windows Server 2008.
        ///     </para>
        /// </summary>
        PAGE_EXECUTE_WRITECOPY = 128, // 0x00000080

        /// <summary>
        ///     Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a
        ///     STATUS_GUARD_PAGE_VIOLATION exception and
        ///     turn off the guard page status. Guard pages thus act as a one-time access alarm. For more information, see Creating
        ///     Guard Pages.
        ///     <para>
        ///         When an access attempt leads the system to turn off guard page status, the underlying page protection takes
        ///         over.
        ///     </para>
        ///     <para>
        ///         If a guard page exception occurs during a system service, the service typically returns a failure status
        ///         indicator.
        ///     </para>
        ///     <para>This value cannot be used with PAGE_NOACCESS.</para>
        ///     <para>This flag is not supported by the CreateFileMapping function.</para>
        /// </summary>
        PAGE_GUARD = 256, // 0x00000100

        /// <summary>
        ///     Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a
        ///     device. Using the interlocked
        ///     functions with memory that is mapped with SEC_NOCACHE can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///     <para>The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.</para>
        ///     <para>
        ///         The PAGE_NOCACHE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx, or
        ///         VirtualAllocExNuma functions. To
        ///         enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag when calling the
        ///         CreateFileMapping function.
        ///     </para>
        /// </summary>
        PAGE_NOCACHE = 512, // 0x00000200

        /// <summary>
        ///     Sets all pages to be write-combined.
        ///     <para>
        ///         Applications should not use this attribute except when explicitly required for a device. Using the interlocked
        ///         functions with memory that is
        ///         mapped as write-combined can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///     </para>
        ///     <para>The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags.</para>
        ///     <para>
        ///         The PAGE_WRITECOMBINE flag can be used only when allocating private memory with the VirtualAlloc,
        ///         VirtualAllocEx, or VirtualAllocExNuma
        ///         functions. To enable write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when
        ///         calling the CreateFileMapping function.
        ///     </para>
        ///     <para>Windows Server 2003 and Windows XP: This flag is not supported until Windows Server 2003 with SP1.</para>
        /// </summary>
        PAGE_WRITECOMBINE = 1024, // 0x00000400

        /// <summary>
        ///     The page contents that you supply are excluded from measurement with the EEXTEND instruction of the Intel SGX
        ///     programming model.
        /// </summary>
        PAGE_ENCLAVE_UNVALIDATED = 536870912, // 0x20000000

        /// <summary>
        ///     Sets all locations in the pages as invalid targets for CFG. Used along with any execute page protection like
        ///     PAGE_EXECUTE, PAGE_EXECUTE_READ,
        ///     PAGE_EXECUTE_READWRITE and PAGE_EXECUTE_WRITECOPY. Any indirect call to locations in those pages will fail CFG
        ///     checks and the process will be
        ///     terminated. The default behavior for executable pages allocated is to be marked valid call targets for CFG.
        ///     <para>This flag is not supported by the VirtualProtect or CreateFileMapping functions.</para>
        /// </summary>
        PAGE_TARGETS_INVALID = 1073741824, // 0x40000000

        /// <summary>
        ///     Pages in the region will not have their CFG information updated while the protection changes for VirtualProtect.
        ///     For example, if the pages in the
        ///     region was allocated using PAGE_TARGETS_INVALID, then the invalid information will be maintained while the page
        ///     protection changes. This flag is
        ///     only valid when the protection changes to an executable type like PAGE_EXECUTE, PAGE_EXECUTE_READ,
        ///     PAGE_EXECUTE_READWRITE and
        ///     PAGE_EXECUTE_WRITECOPY. The default behavior for VirtualProtect protection change to executable is to mark all
        ///     locations as valid call targets
        ///     for CFG.
        ///     <para>
        ///         The following are modifiers that can be used in addition to the options provided in the previous table,
        ///         except as noted.
        ///     </para>
        /// </summary>
        PAGE_TARGETS_NO_UPDATE = PAGE_TARGETS_INVALID, // 0x40000000

        /// <summary>The page contains a thread control structure (TCS).</summary>
        PAGE_ENCLAVE_THREAD_CONTROL = 2147483648, // 0x80000000

        /// <summary>.</summary>
        PAGE_REVERT_TO_FILE_MAP = PAGE_ENCLAVE_THREAD_CONTROL // 0x80000000
    }
}