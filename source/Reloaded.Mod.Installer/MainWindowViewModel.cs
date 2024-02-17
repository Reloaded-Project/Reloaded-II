using System.IO.Compression;
using System.Net.Http;
using static Reloaded.Mod.Installer.DependencyInstaller.DependencyInstaller;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace Reloaded.Mod.Installer;

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

            if (settings.CreateShortcut)
            {
                CurrentStepDescription = "Creating Shortcut";
                MakeShortcut(shortcutPath, executablePath);
            }

            CurrentStepDescription = "All Set";
            
            if (settings.StartReloaded)
                Process.Start(executablePath);
        }
        catch (TaskCanceledException)
        {
            IOEx.TryDeleteDirectory(settings.InstallLocation);
        }
        catch (Exception e)
        {
            IOEx.TryDeleteDirectory(settings.InstallLocation);
            MessageBox.Show("There was an error in installing Reloaded.\n" +
                            $"Feel free to open an issue on github.com/Reloaded-Project/Reloaded-II if you require support.\n" +
                            $"Message: {e.Message}\n" +
                            $"Stack Trace: {e.StackTrace}", "Error in Installing Reloaded");
        }
    }

    private void MakeShortcut(string shortcutPath, string executablePath)
    {
        var shell = (IShellLink)new ShellLink();
        shell.SetDescription($"Reloaded II");
        shell.SetPath($"\"{executablePath}\"");
        shell.SetWorkingDirectory(Path.GetDirectoryName(executablePath));

        // Save Shortcut
        var file = (IPersistFile)shell;
        file.Save(shortcutPath, false);
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
}