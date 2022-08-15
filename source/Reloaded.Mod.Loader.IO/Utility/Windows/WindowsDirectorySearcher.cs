// ReSharper disable InconsistentNaming


namespace Reloaded.Mod.Loader.IO.Utility.Windows;

/// <summary>
/// Class that provides WinAPI based utility methods for fast file enumeration in directories.
/// </summary>
[SupportedOSPlatform("windows5.1.2600")]
public class NtQueryDirectoryFileSearcher
{
    private const string Prefix = "\\??\\";

    static unsafe delegate* unmanaged[Stdcall]<ref IntPtr, int, ref OBJECT_ATTRIBUTES, ref IO_STATUS_BLOCK, ref long, uint, FileShare, int, uint, IntPtr, uint, IntPtr> NtCreateFilePtr;
    static unsafe delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, ref IO_STATUS_BLOCK, IntPtr, UInt32, UInt32, int, IntPtr, int, uint> NtQueryDirectoryFilePtr;
    static unsafe delegate* unmanaged[Stdcall]<IntPtr, int> NtClosePtr;

    static unsafe NtQueryDirectoryFileSearcher()
    {
        var ntdll = LoadLibraryW("ntdll");
        NtCreateFilePtr = (delegate* unmanaged[Stdcall]<ref IntPtr, int, ref OBJECT_ATTRIBUTES, ref IO_STATUS_BLOCK, ref long, uint, FileShare, int, uint, IntPtr, uint, IntPtr>)GetProcAddress(ntdll, "NtCreateFile");
        NtQueryDirectoryFilePtr = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, IntPtr, ref IO_STATUS_BLOCK, IntPtr, uint, uint, int, IntPtr, int, uint>)GetProcAddress(ntdll, "NtQueryDirectoryFile");
        NtClosePtr = (delegate* unmanaged[Stdcall]<IntPtr, int>)GetProcAddress(ntdll, "NtClose");
    }

    /// <summary>
    /// Retrieves the total contents of a directory.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation suceeded, else false.</returns>
    [SkipLocalsInit]
    public static bool TryGetDirectoryContents(string path, List<FileInformation> files, List<DirectoryInformation> directories)
    {
        return TryGetDirectoryContents_Internal(path, files, directories);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <param name="multithreaded">True if to use multithreading, else false</param>
    /// <returns>True if the operation suceeded, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void GetDirectoryContentsRecursive(string path, List<FileInformation> files, List<DirectoryInformation> directories, bool multithreaded)
    {
        var newFiles = new List<FileInformation>();
        var initialDirectories = new List<DirectoryInformation>();

        var initialDirSuccess = TryGetDirectoryContents(path, newFiles, initialDirectories);
        if (!initialDirSuccess)
            return;

        // Add initial files
        files.AddRange(newFiles);
        directories.AddRange(initialDirectories);
        if (initialDirectories.Count <= 0)
            return;

        if (multithreaded)
        {
            // If multiple directories left, let's then go mutlithread.
            var completedEvent = new ManualResetEventSlim(false);
            using var searcher = new MultithreadedDirectorySearcher(initialDirectories, completedEvent, files, directories);
            searcher.Start();
            while (!searcher.Completed())
                completedEvent.Wait();
        }
        else
        {
            // Loop in single stack until all done.
            var remainingDirectories = new Stack<DirectoryInformation>(initialDirectories);
            while (remainingDirectories.TryPop(out var dir))
            {
                newFiles.Clear();
                initialDirectories.Clear();
                TryGetDirectoryContents_Internal(dir.FullPath, newFiles, initialDirectories);

                // Add to accumulator
                directories.AddRange(initialDirectories);
                files.AddRange(newFiles);

                // Add to remaining dirs
                foreach (var newDir in initialDirectories)
                    remainingDirectories.Push(newDir);
            }
        }
    }

    /// <summary>
    /// Retrieves the total contents of a directory for a single directory.
    /// </summary>
    /// <param name="dirPath">The path for which to get the directory for. Must be full path.</param>
    /// <param name="files">The files present in this directory.</param>
    /// <param name="directories">The directories present in this directory.</param>
    /// <returns>True on success, else false.</returns>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static unsafe bool TryGetDirectoryContents_Internal(string dirPath, List<FileInformation> files, List<DirectoryInformation> directories)
    {
        // 128K seemed to have good enough size to fill most queries in while still preserving stack.
        const int BufferSize = 1024 * 16;
        const uint STATUS_SUCCESS = 0x00000000;

        const uint FILE_ATTRIBUTE_NORMAL = 128;

        const int FILE_DIRECTORY_INFORMATION = 1;

        const uint FILE_OPEN = 1;
        const int FILE_SYNCHRONOUS_IO_NONALERT = 0x00000020;

        const int FILE_LIST_DIRECTORY = 0x00000001;
        const int SYNCHRONIZE = 0x00100000;

        // Note: Thanks to SkipLocalsInit, this memory is not zero'd so the allocation is virtually free.
        byte* bufferPtr = stackalloc byte[BufferSize];

        // Add prefix if needed.
        var originalDirPath = dirPath;
        if (!dirPath.StartsWith(Prefix))
            dirPath = $"{Prefix}{dirPath}";

        // Open the folder for reading.
        var hFolder = IntPtr.Zero;
        var objectAttributes = new OBJECT_ATTRIBUTES
        {
            Length = sizeof(OBJECT_ATTRIBUTES),
            Attributes = 0,
            RootDirectory = IntPtr.Zero,
            SecurityDescriptor = IntPtr.Zero,
            SecurityQualityOfService = IntPtr.Zero
        };

        var statusBlock = new IO_STATUS_BLOCK();
        long allocSize = 0;
        var result = IntPtr.Zero;

        fixed (char* dirString = dirPath)
        {
            var objectName = new UNICODE_STRING(dirString, dirPath.Length);
            objectAttributes.ObjectName = &objectName;

            result = NtCreateFile(ref hFolder, FILE_LIST_DIRECTORY | SYNCHRONIZE, ref objectAttributes, ref statusBlock, ref allocSize, FILE_ATTRIBUTE_NORMAL, FileShare.Read, FILE_DIRECTORY_INFORMATION, FILE_OPEN | FILE_SYNCHRONOUS_IO_NONALERT, IntPtr.Zero, 0);
        }

        if ((ulong)result != STATUS_SUCCESS)
            return false;

        try
        {
            // Read remaining files while possible.
            bool moreFiles = true;
            while (moreFiles)
            {
                statusBlock = new IO_STATUS_BLOCK();
                var ntstatus = NtQueryDirectoryFile(hFolder,   // Our directory handle.
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref statusBlock,  // Pointers we don't care about 
                    (IntPtr)bufferPtr, BufferSize, FILE_DIRECTORY_INFORMATION, // Buffer info.
                    0, IntPtr.Zero, 0);

                var currentBufferPtr = (IntPtr)bufferPtr;
                if (ntstatus != STATUS_SUCCESS)
                {
                    moreFiles = false;
                }
                else
                {
                    FILE_DIRECTORY_INFORMATION* info = default;
                    do
                    {
                        info = (FILE_DIRECTORY_INFORMATION*)currentBufferPtr;

                        // Not symlink or symlink to offline file.
                        if ((info->FileAttributes & FileAttributes.ReparsePoint) != 0 &&
                            (info->FileAttributes & FileAttributes.Offline) == 0)
                            goto nextfile;

                        var fileName = Marshal.PtrToStringUni(currentBufferPtr + sizeof(FILE_DIRECTORY_INFORMATION), (int)info->FileNameLength / 2);

                        if (fileName == "." || fileName == "..")
                            goto nextfile;

                        var isDirectory = (info->FileAttributes & FileAttributes.Directory) > 0;
                        if (isDirectory)
                        {
                            directories.Add(new DirectoryInformation
                            {
                                FullPath = $@"{originalDirPath}\{fileName}",
                                LastWriteTime = info->LastWriteTime.ToDateTime()
                            });
                        }
                        else if (!isDirectory)
                        {
                            files.Add(new FileInformation
                            {
                                DirectoryPath = originalDirPath,
                                FileName = fileName,
                                LastWriteTime = info->LastWriteTime.ToDateTime()
                            });
                        }

                        nextfile:
                        currentBufferPtr += (int)info->NextEntryOffset;
                    }
                    while (info->NextEntryOffset != 0);
                }
            }
        }
        finally
        {
            NtClose(hFolder);
        }

        return true;
    }

    internal struct MultithreadedDirectorySearcher : IDisposable
    {
        private Thread[] _threads;
        private bool[] _threadCompleted;
        private ManualResetEventSlim _threadReset;
        private ConcurrentQueue<DirectoryInformation> _remainingDirectories;
        private ManualResetEventSlim _checkCompleted;
        private readonly List<FileInformation> _files;
        private readonly List<DirectoryInformation> _directories;
        private SemaphoreSlim _singleThreadSemaphore = new SemaphoreSlim(1);

        public MultithreadedDirectorySearcher(IEnumerable<DirectoryInformation> initialDirectories, ManualResetEventSlim checkCompleted, List<FileInformation> files, List<DirectoryInformation> directories, int numThreads = -1)
        {
            if (numThreads == -1)
                numThreads = Environment.ProcessorCount * 2;

            _checkCompleted = checkCompleted;
            _files = files;
            _directories = directories;
            _threads = GC.AllocateUninitializedArray<Thread>(numThreads);
            _threadReset = new ManualResetEventSlim(false);
            _remainingDirectories = new ConcurrentQueue<DirectoryInformation>(initialDirectories);
            _threadCompleted = new bool[numThreads];

            for (int x = 0; x < numThreads; x++)
            {
                var thread = new Thread(ThreadLogic);
                thread.IsBackground = true;
                thread.Start(x);
                _threads[x] = thread;
            }
        }

        public void Start() => _threadReset.Set();

        /// <summary>
        /// The task is considered to be completed. 
        /// If there is only 1 (last) thread remaining.
        /// That thread being the one to signal the completed check.
        /// </summary>
        public bool Completed()
        {
            foreach (var completed in _threadCompleted)
            {
                if (!completed)
                    return false;
            }

            return true;
        }

        private void ThreadLogic(object threadIdObj)
        {
            var threadId = (int)threadIdObj!;
            _threadReset.Wait();

            var files = new List<FileInformation>();
            var directories = new List<DirectoryInformation>();

            // Get cracking.
            while (_remainingDirectories.TryDequeue(out var result))
            {
                var currentDirectoryCount = directories.Count;
                TryGetDirectoryContents_Internal(result.FullPath, files, directories);

                // Push directories acquired in this thread to global list of searchables.
                _singleThreadSemaphore.Wait();
                for (int x = currentDirectoryCount; x < directories.Count; x++)
                    _remainingDirectories.Enqueue(directories[x]);
                _singleThreadSemaphore.Release();
            }

            // Add thread data to global list.
            _singleThreadSemaphore.Wait();
            _files.AddRange(files);
            _directories.AddRange(directories);
            _singleThreadSemaphore.Release();

            // Ask master thread to check.
            _threadCompleted[threadId] = true;
            _checkCompleted.Set();
        }

        public void Dispose()
        {
            _threadReset.Dispose();
            _checkCompleted.Dispose();
            _singleThreadSemaphore.Dispose();
        }
    }

    #region P/Invoke
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    #endregion

    #region Native Import Wrappers
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe IntPtr NtCreateFile(ref IntPtr handle, int access, ref OBJECT_ATTRIBUTES objectAttributes,
        ref IO_STATUS_BLOCK ioStatus, ref long allocSize, uint fileAttributes, FileShare share, int createDisposition,
        uint createOptions, IntPtr eaBuffer, uint eaLength)
    {
        return NtCreateFilePtr(ref handle, access, ref objectAttributes, ref ioStatus, ref allocSize, fileAttributes, share, createDisposition, createOptions, eaBuffer, eaLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe uint NtQueryDirectoryFile(IntPtr FileHandle, IntPtr Event, IntPtr ApcRoutine, IntPtr ApcContext,
        ref IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, UInt32 Length, UInt32 FileInformationClass, int BoolReturnSingleEntry,
        IntPtr FileName, int BoolRestartScan)
    {
        return NtQueryDirectoryFilePtr(FileHandle, Event, ApcRoutine, ApcContext, ref IoStatusBlock, FileInformation, Length, FileInformationClass, BoolReturnSingleEntry, FileName, BoolRestartScan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal static unsafe int NtClose(IntPtr hObject) => NtClosePtr(hObject);
    #endregion


    #region Native Structs
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    // ReSharper disable InconsistentNaming
    internal struct LARGE_INTEGER
    {
        [FieldOffset(0)]
        internal Int64 QuadPart;
        [FieldOffset(0)]
        internal Int32 LowPart;
        [FieldOffset(4)]
        internal UInt32 HighPart;

        public DateTime ToDateTime()
        {
            ulong high = (ulong)HighPart;
            ulong low = (ulong)LowPart;
            long fileTime = (long)((high << 32) + low);
            return DateTime.FromFileTimeUtc(fileTime);
        }
    }

    /// <summary>
    /// The OBJECT_ATTRIBUTES structure specifies attributes that can be applied to objects or object
    /// handles by routines that create objects and/or return handles to objects.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct OBJECT_ATTRIBUTES
    {
        /// <summary>
        /// Length of this structure.
        /// </summary>
        public int Length;

        /// <summary>
        /// Optional handle to the root object directory for the path name specified by the ObjectName member.
        /// If RootDirectory is NULL, ObjectName must point to a fully qualified object name that includes the full path to the target object.
        /// If RootDirectory is non-NULL, ObjectName specifies an object name relative to the RootDirectory directory.
        /// The RootDirectory handle can refer to a file system directory or an object directory in the object manager namespace.
        /// </summary>
        public IntPtr RootDirectory;

        /// <summary>
        /// Pointer to a Unicode string that contains the name of the object for which a handle is to be opened.
        /// This must either be a fully qualified object name, or a relative path name to the directory specified by the RootDirectory member.
        /// </summary>
        public unsafe UNICODE_STRING* ObjectName;

        /// <summary>
        /// Bitmask of flags that specify object handle attributes. This member can contain one or more of the flags in the following table (See MSDN)
        /// </summary>
        public uint Attributes;

        /// <summary>
        /// Specifies a security descriptor (SECURITY_DESCRIPTOR) for the object when the object is created.
        /// If this member is NULL, the object will receive default security settings.
        /// </summary>
        public IntPtr SecurityDescriptor;

        /// <summary>
        /// Optional quality of service to be applied to the object when it is created.
        /// Used to indicate the security impersonation level and context tracking mode (dynamic or static).
        /// Currently, the InitializeObjectAttributes macro sets this member to NULL.
        /// </summary>
        public IntPtr SecurityQualityOfService;
    }

    /// <summary>
    /// Represents a singular unicode string.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        private IntPtr buffer;

        public unsafe UNICODE_STRING(char* pointer, int length)
        {
            Length = (ushort)(length * 2);
            MaximumLength = (ushort)(Length + 2);
            buffer = (IntPtr)pointer;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
    internal struct FILE_DIRECTORY_INFORMATION
    {
        internal UInt32 NextEntryOffset;
        internal UInt32 FileIndex;
        internal LARGE_INTEGER CreationTime;
        internal LARGE_INTEGER LastAccessTime;
        internal LARGE_INTEGER LastWriteTime;
        internal LARGE_INTEGER ChangeTime;
        internal LARGE_INTEGER EndOfFile;
        internal LARGE_INTEGER AllocationSize;
        internal FileAttributes FileAttributes;
        internal UInt32 FileNameLength;
        // char[] fileName
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct IO_STATUS_BLOCK_UNION
    {
        [FieldOffset(0)]
        internal UInt32 Status;
        [FieldOffset(0)]
        internal IntPtr Pointer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IO_STATUS_BLOCK
    {
        internal IO_STATUS_BLOCK_UNION Union;
        internal UIntPtr Information;
    }
    #endregion

    public struct FileInformation
    {
        public string DirectoryPath;
        public string FileName;
        public DateTime LastWriteTime;
    }

    public struct DirectoryInformation
    {
        public string FullPath;
        public DateTime LastWriteTime;
    }
}