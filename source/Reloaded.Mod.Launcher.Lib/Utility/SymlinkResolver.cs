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
    /// <param name="allowUwp">Resolves UWP application paths.</param>
    public static string GetFinalPathName(string path, bool allowUwp = true)
    {
        // Special Case for UWP/MSStore.
        if (allowUwp)
        {
            try
            {
                var folder = Path.GetDirectoryName(path);
                var manifest = Path.Combine(folder, "appxmanifest.xml");
                if (File.Exists(manifest))
                    return TryGetFilePathFromUWPAppManifest(path, manifest);
            }
            catch (Exception) { }
        }
        
        var h = CreateFile(path, FILE_READ_EA, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
        if (h == INVALID_HANDLE_VALUE)
            throw new Win32Exception();

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

    private static string TryGetFilePathFromUWPAppManifest(string path, string manifest)
    {
        var document = new XmlDocument();
        document.Load(manifest);

        var tag = document.GetElementsByTagName("Identity")[0];
        var packageName = tag.Attributes["Name"].Value;

        // I wish I could use WinRT APIs but support is removed from runtime and the official way cuts off support for Win7/8.1
        var newFolder = GetPowershellPackageInstallLocation(packageName);
        return Path.Combine(newFolder, Path.GetFileName(path));
    }

    private static string GetPowershellPackageInstallLocation(string packageName)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = @"powershell",
            Arguments = $"(Get-AppxPackage {packageName}).InstallLocation",
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        var process = Process.Start(processStartInfo);
        process.WaitForExit();
        var output = process.StandardOutput.ReadToEnd().TrimEnd();
        return output;
    }

    private static string RemoveDevicePrefix(string path)
    {
        const string DevicePrefix = @"\\?\";
        if (path.StartsWith(DevicePrefix))
            path = path.Substring(DevicePrefix.Length);

        return path;
    }
}