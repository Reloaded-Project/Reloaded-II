using System.Xml;
using Reloaded.Mod.Installer.DependencyInstaller.IO;
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

        // Make a CMD script to unprotect all binaries.
        var script = new StringBuilder();
        foreach (var exeFile in exeFiles)
        {
            // Copy exe to strip protection.
            var newExePath = Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileName(exeFile) + ".new");
            script.AppendLine($"copy /y \"{exeFile}\" \"{newExePath}\"");
            
            // Move old exe to .old
            var movedOldExePath = Path.Combine(Path.GetDirectoryName(exePath), Path.GetFileName(exeFile) + ".old");
            script.AppendLine($"move /y \"{exeFile}\" \"{movedOldExePath}\"");
            
            // Replace old exe with new exe
            script.AppendLine($"move /y \"{newExePath}\" \"{exeFile}\"");
            
            // Delete old exe
            script.AppendLine($"del /f /q \"{movedOldExePath}\"");
        }
        
        // Execute the script in game context where we have perms to access the files.
        using var tempDir = new TemporaryFolderAllocation();
        var scriptPath = Path.Combine(tempDir.FolderPath, "script.bat");
        File.WriteAllText(scriptPath, script.ToString());
        
        // Run the script
        var command = $"Invoke-CommandInDesktopPackage -PackageFamilyName \"{packageFamilyName}\" -AppId \"{appId}\" -Command \"cmd.exe\" -Args '/c \"{scriptPath}\"'";
        var processStartInfo = new ProcessStartInfo
        {
            FileName = @"powershell",
            Arguments = command,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(processStartInfo);
        process?.WaitForExit();
        return true;
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
        var applicationTag = document.GetElementsByTagName("Application")[0]!;
        
        appId = applicationTag!.Attributes!["Id"]!.Value;
        packageFamilyName = GetPowershellPackageFamilyName(packageName);
    }

    private static string GetPowershellPackageFamilyName(string packageName)
    {
        // I wish I could use WinRT APIs but support is removed from runtime and the
        // official way cuts off support for Win7/8.1 in main app.
        var processStartInfo = new ProcessStartInfo
        {
            FileName = @"powershell",
            Arguments = $"(Get-AppxPackage {packageName}).PackageFamilyName",
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(processStartInfo);
        process?.WaitForExit();
        var output = process?.StandardOutput.ReadToEnd().TrimEnd();
        if (output == null)
            throw new Exception("Failed to get Package Family Name via PowerShell.");
        
        return output;
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