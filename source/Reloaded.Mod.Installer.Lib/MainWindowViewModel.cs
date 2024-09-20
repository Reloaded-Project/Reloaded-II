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
        // ReSharper disable InconsistentNaming
        const uint MB_OK = 0x0;
        const uint MB_ICONINFORMATION = 0x40;
        // ReSharper restore InconsistentNaming
        
        // Add suffix if needed
        var protonTricksSuffix = GetProtontricksSuffix();
        OverrideInstallLocationForProton(settings, protonTricksSuffix);
        
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
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Reloaded-II.lnk");

            // For Proton, create a shortcut up one folder above install location.
            if (!string.IsNullOrEmpty(protonTricksSuffix))
                shortcutPath = Path.Combine(Path.GetDirectoryName(settings.InstallLocation)!, "Reloaded-II - " + protonTricksSuffix + ".lnk");
            
            if (settings.CreateShortcut)
            {
                CurrentStepDescription = "Creating Shortcut";
                NativeShellLink.MakeShortcut(shortcutPath, executablePath);
            }

            CurrentStepDescription = "All Set";

            // On WINE, overwrite environment variables that may be inherited
            // from host permanently.
            if (WineDetector.IsWine())
            {
                SetEnvironmentVariable("DOTNET_ROOT", "%ProgramFiles%\\dotnet");
                SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_BASE_DIR", "%TEMP%\\.net");
            }
            
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
    private static void OverrideInstallLocationForProton(Settings settings, string protonTricksSuffix)
    {
        if (settings.IsManuallyOverwrittenLocation) return;
        if (string.IsNullOrEmpty(protonTricksSuffix)) return;
        settings.InstallLocation = Path.Combine(GetHomeDesktopDirectoryOnProton(), $"Reloaded-II - {protonTricksSuffix}");
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
    private static string GetHomeDesktopDirectoryOnProton()
    {
        var user = Environment.GetEnvironmentVariable("LOGNAME");
        if (user != null) 
            // TODO: This is a terrible hack.
            return @$"Z:\home\{user}\Desktop";

        Native.MessageBox(IntPtr.Zero, "Cannot determine username for proton installation.\n" +
                                       "Please make sure that 'LOGNAME' environment variable is set.", 
            "Error in Installing Reloaded", 0x0);
        throw new Exception("Terminated because cannot find username.");
    }
}