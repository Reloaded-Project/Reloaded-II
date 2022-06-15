using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;

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
    public static void GetDirectoryContentsRecursive(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
    {
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();
        TryGetDirectoryContents_Internal_Recursive(path, ref files, ref directories);
    }
    
    private static void TryGetDirectoryContents_Internal_Recursive(string path, ref List<FileInformation> files, ref List<DirectoryInformation> directories)
    {
        var initialDirSuccess = TryGetDirectoryContents(path, out var newFiles, out var newDirectories);
        if (!initialDirSuccess)
            return;

        files.AddRange(newFiles);
        directories.AddRange(newDirectories);
        foreach (var directory in newDirectories)
            TryGetDirectoryContents_Internal_Recursive(directory.FullPath, ref files, ref directories);
    }
    
    private static bool TryGetDirectoryContents_Internal(string path, ref List<FileInformation> files, ref List<DirectoryInformation> directories)
    {
        // Init
        path = Path.GetFullPath(path);
        files = new List<FileInformation>();
        directories = new List<DirectoryInformation>();

        // Native Init
        WIN32_FIND_DATAW findData;
        var findHandle = PInvoke.FindFirstFile($@"{path}\*", out findData);
        if (findHandle.DangerousGetHandle() == INVALID_HANDLE_VALUE)
            return false;

        do
        {
            // Get each file name subsequently.
            var fileName = findData.GetFileName();
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
        while (FindNextFile(findHandle.DangerousGetHandle(), out findData));

        if (findHandle.DangerousGetHandle() != INVALID_HANDLE_VALUE)
            PInvoke.FindClose(new FindFileHandle(findHandle.DangerousGetHandle()));

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

    // The import from Microsoft.Windows.Sdk uses a class as paramter; don't want heap allocations.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    internal static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);
}

/// <summary>
/// Extensions to the automatically Source Generator imported WinAPI classes.
/// </summary>
public static class FindDataExtensions
{
    internal static unsafe string GetFileName(this WIN32_FIND_DATAW value)
    {
        return Marshal.PtrToStringUni((IntPtr)(&value.cFileName));
    }
}

/// <summary>
/// Extensions to the internal COM FILETIME class.
/// </summary>
public static class FileTimeExtensions
{
    public static DateTime ToDateTime(this System.Runtime.InteropServices.ComTypes.FILETIME time)
    {
        ulong high = (ulong)time.dwHighDateTime;
        ulong low = (ulong)time.dwLowDateTime;
        long fileTime = (long)((high << 32) + low);
        return DateTime.FromFileTimeUtc(fileTime);
    }
}