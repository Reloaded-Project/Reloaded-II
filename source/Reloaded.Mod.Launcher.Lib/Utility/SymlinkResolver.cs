// ReSharper disable InconsistentNaming

using System.Xml;
using System.Xml.Linq;
using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Utility;

// TODO: Obsolete this class with `FileInfo.LinkTarget` after upgrade to .NET 7.
// This class is temporary, thus not fully optimised/cleaned up.

/// <summary>
/// Handy class for resolving symlinks in Windows.
/// </summary>
public static class SymlinkResolver
{
    private const short MaxPath = short.MaxValue; // Windows 10 with path extension.
    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    private const uint FILE_READ_EA = 0x0008;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
            IntPtr templateFile);

    /// <summary>
    /// Resolves a symbolic link and normalizes the path.
    /// </summary>
    /// <param name="path">The path to be resolved.</param>
    public static string GetFinalPathName(string path)
    {
        // Special Case for UWP/MSStore.
        var h = CreateFile(path, FILE_READ_EA, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
        if (h == INVALID_HANDLE_VALUE)
            return path;

        try
        {
            var stringBuilder = new StringBuilder(MaxPath);
            var res = GetFinalPathNameByHandle(h, stringBuilder, (uint)MaxPath, 0);
            if (res == 0)
                throw new Win32Exception();

            // Use GetFullPath to normalize returned path.
            return RemoveDevicePrefix(stringBuilder.ToString());
        }
        finally
        {
            CloseHandle(h);
        }
    }

    private static string RemoveDevicePrefix(string path)
    {
        const string DevicePrefix = @"\\?\";
        if (path.StartsWith(DevicePrefix))
            path = path.Substring(DevicePrefix.Length);

        return path;
    }
}