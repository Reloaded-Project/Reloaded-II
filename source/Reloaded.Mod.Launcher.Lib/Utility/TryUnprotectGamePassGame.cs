using System.Security.Cryptography;
using System.Xml;
using Reloaded.Mod.Installer.DependencyInstaller.IO;
using static Reloaded.Mod.Launcher.Lib.Utility.DesktopAppxActivateOptions;
using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
///     GamePass games restrict our access to EXE files, making it difficult for Reloaded to do various operations:
///     - Displaying Game Icon
///     - Deploying ASI Loader
///     etc.
/// </summary>
public static class TryUnprotectGamePassGame
{
    /// <summary/>
    /// <param name="exePath">Path to the main game binary.</param>
    /// <returns>True if this was auto-unprotected.</returns>
    public static bool TryIt(string exePath)
    {
        // Note: We assume this may be a GamePass game if we can't read the binary.
        //       This is a very naive approach, but as we're not parsing `.GamingRoot`,
        //       to 'know' that this is a library path, this is the best we can do for now.
        var read = CanRead(exePath);
        if (read)
            return !read;

        // If we can't read it, try finding an AppxManifest.xml file by going up directories.
        if (!GetAppXManifestPath(exePath, out var manifestPath)) 
            return false;

        ExtractInfoFromUWPAppManifest(manifestPath!, out var appId, out var packageFamilyName);

        var contentFolder = Path.GetDirectoryName(manifestPath);
        var exeFiles = Directory.GetFiles(contentFolder!, "*.exe", SearchOption.AllDirectories);

        // Load custom binary that does file copying to handle the decryption.
        var libraryDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        var compressedLoaderPath = $"{libraryDirectory}/Assets/replace-files-with-itself.exe";

        // Append command to create 'terminate' file indicating script completion.
        using var tempDir = new TemporaryFolderAllocation();
        var scriptPath = Path.Combine(tempDir.FolderPath, "files.txt");
        File.WriteAllLines(scriptPath, exeFiles, Encoding.UTF8);

        // Execute the script in game context where we have perms to access the files.
        // ReSharper disable once SuspiciousTypeConversion.Global
        TryActivate(
            packageFamilyName + "!" + appId,
            compressedLoaderPath,
            $"\"{scriptPath}\""
        );

        // Wait until 'replace-files-with-itself' is terminated.
        var processName = "replace-files-with-itself";
        while (Process.GetProcessesByName(processName).Length > 0)
            Thread.Sleep(1);

        return true;
    }

    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    private static void TryActivate(string packageFamilyName, string compressedLoaderPath, string scriptPath)
    {
        // Note: `Get-Command Invoke-CommandInDesktopPackage | Format-List *` in powershell
        //       and decompile with dnSpy, etc. to check current OS impl.
        try
        {
            var act = (IDesktopAppxActivatorWin11)new DesktopAppxActivator();
            act.ActivateWithOptions(
                packageFamilyName, 
                compressedLoaderPath, 
                scriptPath,
                (uint)(CentennialProcess | NonPackagedExeProcessTree),
                0,
                out _);
            return;
        }
        catch (Exception) { /* ignored */ }

        try
        {
            var act = (IDesktopAppxActivatorWin10)new DesktopAppxActivator();
            act.ActivateWithOptions(
                packageFamilyName, 
                compressedLoaderPath, 
                scriptPath,
                (uint)(CentennialProcess | NonPackagedExeProcessTree),
                0,
                out _);
            return;
        }
        catch (Exception) { /* ignored */ }

        throw new Exception("Can't make use of DesktopAppxActivator. Your OS may be too recent. Please report this.");
    }

    /// <summary/>
    /// <param name="exePath">Path to the main game binary.</param>
    /// <returns>True if this was auto-unprotected.</returns>
    public static bool TryIgnoringErrors(string exePath)
    {
        try
        {
            return TryIt(exePath);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool GetAppXManifestPath(string exePath, [MaybeNullWhen(false)] out string appManifest)
    {
        var currentDirectory = Path.GetDirectoryName(exePath);
        while (currentDirectory != null)
        {
            var appxManifestPath = Path.Combine(currentDirectory, "AppxManifest.xml");
            if (File.Exists(appxManifestPath))
            {
                appManifest = appxManifestPath;
                return true;
            }

            currentDirectory = Path.GetDirectoryName(currentDirectory);
        }

        appManifest = null;
        return false;
    }
    
    private static void ExtractInfoFromUWPAppManifest(string manifest, out string appId, out string packageFamilyName)
    {
        var document = new XmlDocument();
        document.Load(manifest);

        var tag = document.GetElementsByTagName("Identity")[0]!;
        var packageName = tag!.Attributes!["Name"]!.Value;
        var publisherName = tag!.Attributes!["Publisher"]!.Value;
        var applicationTag = document.GetElementsByTagName("Application")[0]!;
        
        appId = applicationTag!.Attributes!["Id"]!.Value;
        packageFamilyName = $"{packageName}_{GetPublisherHash(publisherName)}";
    }
    
    // Credits: https://gist.github.com/marcinotorowski/6a51023600160fcceef9ceea341bbc4a
    private static string GetPublisherHash(string publisherId)
    {
        using var sha = SHA256.Create();
        var encoded = sha.ComputeHash(Encoding.Unicode.GetBytes(publisherId));
        var binaryString = string.Concat(encoded.Take(8).Select(c => Convert.ToString(c, 2).PadLeft(8, '0'))) + '0'; // representing 65-bits = 13 * 5
        var encodedPublisherId = string.Concat(Enumerable.Range(0, binaryString.Length / 5).Select(i => "0123456789abcdefghjkmnpqrstvwxyz".Substring(Convert.ToInt32(binaryString.Substring(i * 5, 5), 2), 1)));
        return encodedPublisherId;
    }

    private static bool CanRead(string exePath)
    {
        try
        {
            using var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 524288);
            fs.ReadByte();
            return true;
        }
        catch
        {
            // We can't read the file.
            return false;
        }
    }
}

// Enum for activation options
[Flags]
internal enum DesktopAppxActivateOptions
{
    None = 0,
    Elevate = 1,
    NonPackagedExe = 2,
    NonPackagedExeProcessTree = 4,
    NonPackagedExeFlags = 6,
    NoErrorUI = 8,
    CheckForAppInstallerUpdates = 16,
    CentennialProcess = 32,
    UniversalProcess = 64,
    Win32AlaCarteProcess = 128,
    RuntimeBehaviorFlags = 224,
    PartialTrust = 256,
    UniversalConsole = 512,
    AppSilo = 1024,
    TrustLevelFlags = 1280,
}

// COM interface for activating desktop applications
[Guid("F158268A-D5A5-45CE-99CF-00D6C3F3FC0A")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDesktopAppxActivatorWin11
{
    void Activate(
        [MarshalAs(UnmanagedType.LPWStr)]
        string applicationUserModelId,
        [MarshalAs(UnmanagedType.LPWStr)]
        string packageRelativeExecutable,
        [MarshalAs(UnmanagedType.LPWStr)]
        string arguments,
        out IntPtr processHandle);

    void ActivateWithOptions(
        [MarshalAs(UnmanagedType.LPWStr)]
        string applicationUserModelId,
        [MarshalAs(UnmanagedType.LPWStr)]
        string executable,
        [MarshalAs(UnmanagedType.LPWStr)]
        string arguments,
        uint activationOptions,
        uint parentProcessId,
        out IntPtr processHandle);
}

[Guid("72e3a5b0-8fea-485c-9f8b-822b16dba17f")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDesktopAppxActivatorWin10
{
    void Activate(
        [MarshalAs(UnmanagedType.LPWStr)]
        string applicationUserModelId,
        [MarshalAs(UnmanagedType.LPWStr)]
        string packageRelativeExecutable,
        [MarshalAs(UnmanagedType.LPWStr)]
        string arguments,
        out IntPtr processHandle);

    void ActivateWithOptions(
        [MarshalAs(UnmanagedType.LPWStr)]
        string applicationUserModelId,
        [MarshalAs(UnmanagedType.LPWStr)]
        string executable,
        [MarshalAs(UnmanagedType.LPWStr)]
        string arguments,
        uint activationOptions,
        uint parentProcessId,
        out IntPtr processHandle);
}

[ComImport]
[Guid("168EB462-775F-42AE-9111-D714B2306C2E")]
class DesktopAppxActivator
{
    
}