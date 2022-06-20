using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
// ReSharper disable InconsistentNaming

namespace Reloaded.Mod.Loader.IO.Utility.Windows;

/// <summary>
/// Class that provides WinAPI based utility methods for fast file enumeration in directories.
/// </summary>
[SupportedOSPlatform("windows5.1.2600")]
public static class WindowsDirectorySearcher
{
    private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    /// <summary>
    /// Retrieves the total contents of a directory.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation suceeded, else false.</returns>
    public static bool TryGetDirectoryContents(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        return TryGetDirectoryContents_Internal(path, ref files, ref directories);
    }

    /// <summary>
    /// Retrieves the total contents of a directory and all sub directories.
    /// </summary>
    /// <param name="path">The path to search inside. Should not end with a backslash.</param>
    /// <param name="files">Files contained inside the target directory.</param>
    /// <param name="directories">Directories contained inside the target directory.</param>
    /// <returns>True if the operation suceeded, else false.</returns>
    [SkipLocalsInit]
    public static void GetDirectoryContentsRecursive(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        TryGetDirectoryContents_Internal_Recursive(path, ref files, ref directories);
    }

    [SkipLocalsInit]
    private static void TryGetDirectoryContents_Internal_Recursive(string path, ref List<FileInformation> files, ref List<DirectoryInformation> directories)
    {
        var initialDirSuccess = TryGetDirectoryContents(path, out var newFiles, out var newDirectories);
        if (!initialDirSuccess)
            return;

        files.AddRange(newFiles);
        directories.AddRange(newDirectories);

        newFiles = null;
        foreach (var directory in newDirectories)
            TryGetDirectoryContents_Internal_Recursive(directory.FullPath, ref files, ref directories);
    }

    [SkipLocalsInit]
    private static unsafe bool TryGetDirectoryContents_Internal(string path, ref List<FileInformation> files, ref List<DirectoryInformation> directories)
    {
        // Init
        path = Path.GetFullPath(path);
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();

        // Native Init
        Win32FindData findData;
        var findHandle = FindFirstFileExW($@"{path}\*", FINDEX_INFO_LEVELS.FindExInfoBasic, &findData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, 0);
        if (findHandle == INVALID_HANDLE_VALUE)
            return false;

        do
        {
            // Get each file name subsequently.
            var fileName = Marshal.PtrToStringUni((IntPtr)(&findData.cFileName));
            if (fileName == "." || fileName == "..")
                continue;

            string fullPath = $@"{path}\{fileName}";

            // Check if this is a directory and not a symbolic link since symbolic links
            // could lead to repeated files and folders as well as infinite loops.
            var attributes = (FileAttributes)findData.dwFileAttributes;
            bool isDirectory = attributes.HasFlag(FileAttributes.Directory);

            if (isDirectory && !attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                directories.Add(new DirectoryInformation
                {
                    FullPath = fullPath,
                    LastWriteTime = findData.ftLastWriteTime.ToDateTime()
                });
            }
            else if (!isDirectory)
            {
                files.Add(new FileInformation
                {
                    FullPath = fullPath,
                    LastWriteTime = findData.ftLastWriteTime.ToDateTime()
                });
            }
        }
        while (FindNextFileW(findHandle, &findData));

        if (findHandle != INVALID_HANDLE_VALUE)
            FindClose(findHandle);

        return true;
    }

    public struct FileInformation
    {
        public string FullPath;
        public DateTime LastWriteTime;
    }

    public struct DirectoryInformation
    {
        public string FullPath;
        public DateTime LastWriteTime;
    }
    
    #region P/Invoke Definitions
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern unsafe IntPtr FindFirstFileExW(string lpFileName, FINDEX_INFO_LEVELS levels, Win32FindData* lpFindFileData, FINDEX_SEARCH_OPS fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern unsafe bool FindNextFileW(IntPtr hFindFile, Win32FindData* lpFindFileData);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FindClose(IntPtr hFindFile);

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto, Size = 65578)] // Up to 32767 chars
    public struct Win32FindData
    {
        [FieldOffset(0)]
        public FileAttributes dwFileAttributes;

        [FieldOffset(4)]
        public FileTime ftCreationTime;

        [FieldOffset(12)]
        public FileTime ftLastAccessTime;

        [FieldOffset(20)]
        public FileTime ftLastWriteTime;

        [FieldOffset(28)]
        public uint nFileSizeHigh;

        [FieldOffset(32)]
        public uint nFileSizeLow;

        [FieldOffset(36)]
        public uint dwReserved0;

        [FieldOffset(40)]
        public uint dwReserved1;

        [FieldOffset(44)]
        public IntPtr cFileName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FileTime
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    };

    public enum FINDEX_INFO_LEVELS
    {
        FindExInfoStandard,
        FindExInfoBasic,
        FindExInfoMaxInfoLevel
    }

    public enum FINDEX_SEARCH_OPS
    {
        FindExSearchNameMatch = 0,
        FindExSearchLimitToDirectories = 1,
        FindExSearchLimitToDevices = 2
    }
    #endregion
}

/// <summary>
/// Extensions to the internal COM FILETIME class.
/// </summary>
public static class FileTimeExtensions
{
    public static DateTime ToDateTime(this in WindowsDirectorySearcher.FileTime time)
    {
        ulong high = (ulong)time.dwHighDateTime;
        ulong low = (ulong)time.dwLowDateTime;
        long fileTime = (long)((high << 32) + low);
        return DateTime.FromFileTimeUtc(fileTime);
    }
}