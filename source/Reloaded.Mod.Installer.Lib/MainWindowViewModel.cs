using System.IO.Compression;
using System.Net.Http;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;
using Microsoft.Win32;
using Reloaded.Mod.Installer.Lib.Utilities;
using static Windows.Win32.PInvoke;
using static Reloaded.Mod.Installer.DependencyInstaller.DependencyInstaller;
using Path = System.IO.Path;

namespace Reloaded.Mod.Installer.Lib;

public class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// The current step of the download process.
    /// </summary>
    public int CurrentStepNo { get; set; }

    /// <summary>
    /// Current production step.
    /// </summary>
    public string CurrentStepDescription { get; set; } = "";

    /// <summary>
    /// The current setup progress.
    /// Range 0 - 100.0f.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// A cancellation token allowing you to cancel the download operation.
    /// </summary>
    public CancellationTokenSource CancellationToken { get; } = new();

    public async Task InstallReloadedAsync(Settings settings)
    {
        // Note: All of this code is terrible, I don't have the time to make it good.
        
        // ReSharper disable InconsistentNaming
        const uint MB_OK = 0x0;
        const uint MB_ICONINFORMATION = 0x40;
        // ReSharper restore InconsistentNaming
        
        // Handle Proton specific customizations.
        var protonTricksSuffix = GetProtontricksSuffix();
        var isProton = !string.IsNullOrEmpty(protonTricksSuffix);
        OverrideInstallLocationForProton(settings, protonTricksSuffix, out var nativeInstallFolder, out var userName);
        
        // Check for existing installation
        if (Directory.Exists(settings.InstallLocation) && Directory.GetFiles(settings.InstallLocation).Length > 0)
        {
            Native.MessageBox(IntPtr.Zero, $"An existing installation has been detected at:\n{settings.InstallLocation}\n\n" +
                                           $"To prevent data loss, installation will be aborted.\n" +
                                           $"If you wish to reinstall, delete or move the existing installation.", "Existing Installation", MB_OK | MB_ICONINFORMATION);
            return;
        }
        
        // Step
        Directory.CreateDirectory(settings.InstallLocation);

        using var tempDownloadDir = new TemporaryFolderAllocation();
        var progressSlicer = new ProgressSlicer(new Progress<double>(d =>
        { 
            Progress = d * 100.0;
        }));

        try
        {
            var downloadLocation = Path.Combine(tempDownloadDir.FolderPath, $"Reloaded-II.zip");

            // 0.15
            CurrentStepNo = 0;
            await DownloadReloadedAsync(downloadLocation, progressSlicer.Slice(0.15));
            if (CancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            // 0.20
            CurrentStepNo = 1;
            ExtractReloaded(settings.InstallLocation, downloadLocation, progressSlicer.Slice(0.05));
            if (CancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            // 1.00
            CurrentStepNo = 2;
            await CheckAndInstallMissingRuntimesAsync(settings.InstallLocation, tempDownloadDir.FolderPath,
                progressSlicer.Slice(0.8),
                s => { CurrentStepDescription = s; }, CancellationToken.Token);

            var executableName = IntPtr.Size == 8 ? "Reloaded-II.exe" : "Reloaded-II32.exe";
            var executablePath = Path.Combine(settings.InstallLocation, executableName);
            var nativeExecutablePath = Path.Combine(nativeInstallFolder, executableName);
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Reloaded-II.lnk");

            // For Proton/Wine, create a shortcut up one folder above install location.
            if (!string.IsNullOrEmpty(protonTricksSuffix))
                shortcutPath = Path.Combine(Path.GetDirectoryName(settings.InstallLocation)!, "Reloaded-II - " + protonTricksSuffix + ".lnk");
            
            if (settings.CreateShortcut)
            {
                CurrentStepDescription = "Creating Shortcut";
                if (!isProton)
                    NativeShellLink.MakeShortcut(shortcutPath, executablePath);
                else
                    MakeProtonShortcut(userName, protonTricksSuffix, shortcutPath, nativeExecutablePath);
            }

            CurrentStepDescription = "All Set";

            // On WINE, overwrite environment variables that may be inherited
            // from host permanently.
            if (WineDetector.IsWine())
            {
                ShowDotFilesInWine();
                SetEnvironmentVariable("DOTNET_ROOT", "%ProgramFiles%\\dotnet");
                SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_BASE_DIR", "%TEMP%\\.net");
            }

            if (!settings.HideNonErrorGuiMessages && isProton)
                Native.MessageBox(IntPtr.Zero, $"Reloaded was installed via Proton to your Desktop.\nYou can find it at: {nativeInstallFolder}", "Installation Complete", MB_OK | MB_ICONINFORMATION);
            
            if (settings.StartReloaded)
            {
                // We're in an admin process; but we want to de-escalate as Reloaded-II is not
                // meant to be used in admin mode.
                StartProcessWithReducedPrivileges(executablePath);
            }
        }
        catch (TaskCanceledException)
        {
            IOEx.TryDeleteDirectory(settings.InstallLocation);
        }
        catch (Exception e)
        {
            IOEx.TryDeleteDirectory(settings.InstallLocation);
            Native.MessageBox(IntPtr.Zero, "There was an error in installing Reloaded.\n" +
                              $"Feel free to open an issue on github.com/Reloaded-Project/Reloaded-II if you require support.\n" +
                              $"Message: {e.Message}\n" +
                              $"Stack Trace: {e.StackTrace}", "Error in Installing Reloaded", MB_OK);
        }
    }
    private void MakeProtonShortcut(string? userName, string protonTricksSuffix, string shortcutPath, string nativeExecutablePath)
    {
        nativeExecutablePath = nativeExecutablePath.Replace('\\', '/');
        var desktopFile = 
"""
[Desktop Entry]
Name=Reloaded-II ({SUFFIX})
Exec=protontricks-launch --appid {APPID} "{NATIVEPATH}"
Type=Application
StartupNotify=true
Comment=Reloaded II installation for {SUFFIX}
Path={RELOADEDFOLDER}
Icon={RELOADEDFOLDER}/Mods/reloaded.sharedlib.hooks/Preview.png
StartupWMClass=reloaded-ii.exe
""";
        // reloaded.sharedlib.hooks is present in all Reloaded installs after boot, so we can use that... for now.
        desktopFile = desktopFile.Replace("{USER}", userName);
        desktopFile = desktopFile.Replace("{APPID}", Environment.GetEnvironmentVariable("STEAM_APPID"));
        desktopFile = desktopFile.Replace("{SUFFIX}", protonTricksSuffix);
        desktopFile = desktopFile.Replace("{RELOADEDFOLDER}", Path.GetDirectoryName(nativeExecutablePath)!.Replace('\\', '/'));
        desktopFile = desktopFile.Replace("{NATIVEPATH}", nativeExecutablePath);
        shortcutPath = shortcutPath.Replace(".lnk", ".desktop");
        
        File.WriteAllText(shortcutPath, desktopFile);

        // Write `.desktop` file that integrates into shell.
        var shellShortcutPath = $@"Z:\home\{userName}\.local\share\applications\{Path.GetFileName(shortcutPath)}";
        File.WriteAllText(shellShortcutPath, desktopFile);
        
        // Mark as executable.
        LinuxTryMarkAsExecutable(shortcutPath);
        LinuxTryMarkAsExecutable(shellShortcutPath);
    }

    private static void LinuxTryMarkAsExecutable(string windowsPath)
    {
        windowsPath = windowsPath.Replace('\\', '/');
        windowsPath = windowsPath.Replace("Z:", "");
        var processInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c start Z:/bin/chmod +x \"{windowsPath}\"",
            UseShellExecute = true,
            CreateNoWindow = true
        };
        try 
        { 
            Process.Start(processInfo); 
        }
        catch (Exception) 
        { 
            // If the first attempt fails, try with the alternative path
            processInfo.Arguments = $"/c start Z:/usr/bin/chmod +x \"{windowsPath}\"";
            try 
            { 
                Process.Start(processInfo); 
            }
            catch (Exception) 
            { 
                // Both attempts failed
            }
        }
    }

    private static void OverrideInstallLocationForProton(Settings settings, string protonTricksSuffix, out string nativeInstallFolder, out string? userName)
    {
        nativeInstallFolder = "";
        userName = "";
        if (settings.IsManuallyOverwrittenLocation) return;
        if (string.IsNullOrEmpty(protonTricksSuffix)) return;

        var desktopDir = GetHomeDesktopDirectoryOnProton(out nativeInstallFolder, out userName);
        var folderName = $"Reloaded-II - {protonTricksSuffix}";

        settings.InstallLocation = Path.Combine(desktopDir, folderName);
        nativeInstallFolder = Path.Combine(nativeInstallFolder, folderName);
    }

    private static async Task DownloadReloadedAsync(string downloadLocation, IProgress<double> downloadProgress)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync("https://github.com/Reloaded-Project/Reloaded-II/releases/latest/download/Release.zip", HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0L;
        var totalReadBytes = 0L;
        int readBytes;
        var buffer = new byte[128 * 1024];
        
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, true);
        while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, readBytes);
            totalReadBytes += readBytes;
            downloadProgress?.Report(totalReadBytes * 1d / totalBytes);
        }

        if (!File.Exists(downloadLocation))
            throw new Exception("Reloaded failed to download (no file was written to disk).");
    }

    private static void ExtractReloaded(string extractFolderPath, string downloadedPackagePath, IProgress<double> slice)
    {
        ZipFile.ExtractToDirectory(downloadedPackagePath, extractFolderPath);
        if (Directory.GetFiles(extractFolderPath).Length == 0)
            throw new Exception($"Reloaded failed to download (downloaded archive was not properly extracted).");

        File.Delete(downloadedPackagePath);
        slice.Report(1);
    }
    
    private static unsafe void StartProcessWithReducedPrivileges(string executablePath)
    {
        SAFER_LEVEL_HANDLE saferHandle = default;
        try
        {
            // 1. Create a new new access token
            if (!SaferCreateLevel(SAFER_SCOPEID_USER, SAFER_LEVELID_NORMALUSER,
                    SAFER_LEVEL_OPEN, &saferHandle, null))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!SaferComputeTokenFromLevel(saferHandle, null, out var newAccessToken, 0, null))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // Set the token to medium integrity because SaferCreateLevel doesn't reduce the
            // integrity level of the token and keep it as high.
            if (!ConvertStringSidToSid("S-1-16-8192", out var psid))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            TOKEN_MANDATORY_LABEL tml = default;
            tml.Label.Attributes = SE_GROUP_INTEGRITY;
            tml.Label.Sid = (PSID)psid.DangerousGetHandle();

            var length = (uint)Marshal.SizeOf(tml);
            if (!SetTokenInformation(newAccessToken, TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, &tml, length))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // 2. Start process using the new access token
            // Cannot use Process.Start as there is no way to set the access token to use
            fixed (char* commandLinePtr = executablePath)
            {
                STARTUPINFOW si = default;
                Span<char> span = new Span<char>(commandLinePtr, executablePath.Length);
                if (CreateProcessAsUser(newAccessToken, null, ref span, null, null, bInheritHandles: false, default,
                        null, null, in si, out var pi))
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                }
            }

        }
        catch
        {
            // In case of WINE, or some other unexpected event.
            Process.Start(executablePath);
        }
        finally
        {
            if (saferHandle != default)
            {
                SaferCloseLevel(saferHandle);
            }
        }
    }
    
    private static void SetEnvironmentVariable(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        using var key = Registry.CurrentUser.OpenSubKey("Environment", true);
        if (key != null)
        {
            key.SetValue(name, value);
            Console.WriteLine($"Environment variable '{name}' set to '{value}'");
        }
        else
        {
            Console.WriteLine("Failed to open Environment key in registry");
        }
    }

    /// <summary>
    /// This suffix is appended to shortcut name and install folder.
    /// </summary>
    private static string GetProtontricksSuffix()
    {
        try
        {
            // Note: Steam games are usually installed in a folder which is a friendly name
            //       for the game. If the user is running in Protontricks, there's a high
            //       chance that the folder will be named just right, e.g. 'Persona 5 Royal'.
            return Path.GetFileName(Environment.GetEnvironmentVariable("STEAM_APP_PATH")) ?? string.Empty;
        }
        catch (Exception)
        {
            return "";
        }
    }
    
    /// <summary>
    /// This suffix is appended to shortcut name and install folder.
    /// </summary>
    private static string GetHomeDesktopDirectoryOnProton(out string linuxPath, out string? userName)
    {
        userName = Environment.GetEnvironmentVariable("LOGNAME");
        if (userName != null)
        {
            // TODO: This is a terrible hack.
            linuxPath = $"/home/{userName}/Desktop";
            return @$"Z:\home\{userName}\Desktop";
        }

        Native.MessageBox(IntPtr.Zero, "Cannot determine username for proton installation.\n" +
                                       "Please make sure that 'LOGNAME' environment variable is set.", 
            "Error in Installing Reloaded", 0x0);
        throw new Exception("Terminated because cannot find username.");
    }

    private static void ShowDotFilesInWine()
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Wine", true)!;
            // Set the ShowDotFiles value to "Y"
            key.SetValue("ShowDotFiles", "Y", RegistryValueKind.String);
            Console.WriteLine("Successfully set ShowDotFiles to Y in the Wine registry.");
        }
        catch (Exception)
        {
            Native.MessageBox(IntPtr.Zero, "Failed to auto-unhide dot files in Wine.\n" +
                                           "You'll need to enter `winecfg` and check `show dot files` manually yourself.", 
                "Error in Configuring WINE", 0x0);
        }
    }
}