using System.Diagnostics;

namespace Reloaded.Mod.Installer.DependencyInstaller;

public static class DependencyInstaller
{
    /// <summary>
    /// Gets all URLs to missing runtime files.
    /// </summary>
    /// <param name="reloadedFolderPath">Folder path for an application, library will check every runtimeconfig.json inside this folder.</param>
    public static async Task<HashSet<UrlCommandlinePair>> GetMissingRuntimeUrls(string reloadedFolderPath) 
    {
        var allFiles = Directory.GetFiles(reloadedFolderPath, "*.*", SearchOption.AllDirectories);
        var filesWithRuntimeConfigs = allFiles.Where(x => x.EndsWith(".runtimeconfig.json")).ToArray();

        // Now that we have the runtime config files, get all missing deps from them.
        var urlSet = new HashSet<UrlCommandlinePair>();
        var dotNetCommandLineParams = new string[]
        {
            "/install",
            "/quiet",
            "/norestart"
        };

        foreach (var is64Bit in new[] { true, false })
        {
            var architecture = is64Bit ? Architecture.Amd64 : Architecture.x86;
            var finder = new FrameworkFinder(is64Bit);
            var resolver = new DependencyResolver(finder);
            foreach (var runtimeConfig in filesWithRuntimeConfigs)
            {
                var runtimeOptions = RuntimeOptions.FromFile(runtimeConfig);
                var result = resolver.Resolve(runtimeOptions); ;
                foreach (var missingDep in result.MissingDependencies)
                {
                    var frameworkDownloader = new FrameworkDownloader(runtimeOptions.GetAllFrameworks()[0].NuGetVersion, missingDep.FrameworkName);
                    urlSet.Add(new UrlCommandlinePair()
                    {
                        FriendlyName = $".NET Core {runtimeOptions.GetAllFrameworks()[0].Version} {architecture}",
                        Url = await frameworkDownloader.GetDownloadUrlAsync(architecture),
                        Parameters = dotNetCommandLineParams
                    });
                }
            }
        }

        // Also check for C++ Runtime.
        if (!RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x64))
        {
            urlSet.Add(new UrlCommandlinePair()
            {
                FriendlyName = "Visual C++ Redist x64",
                Url = "https://aka.ms/vs/17/release/vc_redist.x64.exe",
                Parameters = dotNetCommandLineParams
            });
        }

        if (!RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x86))
        {
            urlSet.Add(new UrlCommandlinePair()
            {
                FriendlyName = "Visual C++ Redist x86",
                Url = "https://aka.ms/vs/17/release/vc_redist.x86.exe",
                Parameters = dotNetCommandLineParams
            });
        }

        return urlSet;
    }

    /// <summary>
    /// [Requires Admin Permission]
    /// Finds all missing runtimes and silently installs them.
    /// </summary>
    /// <param name="folderPath">Path to the folder to check all .NET Core Runtime configurations.</param>
    /// <param name="downloadDirPath">Path to where the temporary runtimes should be downloaded to.</param>
    /// <param name="progress">Progress for the installation operation.</param>
    /// <param name="setCurrentStepDescription">Shows a description of the current runtime installation step.</param>
    /// <param name="token">Token allowing you to cancel the install task.</param>
    public static async Task CheckAndInstallMissingRuntimesAsync(string folderPath, string downloadDirPath, IProgress<double> progress, Action<string> setCurrentStepDescription = null, CancellationToken token = default)
    {
        // Get files with runtime configurations.
        setCurrentStepDescription?.Invoke("Searching for Missing Runtimes");
        var urlSet = await GetMissingRuntimeUrls(folderPath);

        await DownloadAndInstallRuntimesAsync(urlSet, downloadDirPath, progress, setCurrentStepDescription, token);
    }


    /// <summary>
    /// [Requires Admin Permission]
    /// Finds all missing runtimes and silently installs them.
    /// </summary>
    /// <param name="urlSet">Set of all URLs and commandline arguments for the downloaded files. Obtained form running <see cref="GetMissingRuntimeUrls"/>.</param>
    /// <param name="downloadDirPath">Path to where the temporary runtimes should be downloaded to.</param>
    /// <param name="progress">Progress for the installation operation.</param>
    /// <param name="setCurrentStepDescription">Shows a description of the current runtime installation step.</param>
    /// <param name="token">Token allowing you to cancel the install task.</param>
    public static async Task DownloadAndInstallRuntimesAsync(HashSet<UrlCommandlinePair> urlSet, string downloadDirPath, IProgress<double> progress, Action<string> setCurrentStepDescription, CancellationToken token = default)
    {
        // Download Runtimes 
        setCurrentStepDescription?.Invoke("Downloading & Installing Runtimes");
        var progressSlicer = new ProgressSlicer(progress);
        var downloadSlice = progressSlicer.Slice(0.8f);
        var installSlice = progressSlicer.Slice(0.2f);

        var downloadQueue = new ConcurrentQueue<UrlCommandlinePair>(urlSet);
        var installQueue = new ConcurrentQueue<UrlCommandlinePair>();

        // Start background download and install.
        var downloadTask = DownloadAllRuntimesAsync();

        async Task DownloadAllRuntimesAsync()
        {
            var queueElements = downloadQueue.Count;
            var downloadSlicer = new ProgressSlicer(downloadSlice);
            int x = 0;

            while (downloadQueue.TryDequeue(out UrlCommandlinePair downloadLink))
            {
                var client = new WebClient();
                var downloadProgress = downloadSlicer.Slice(1.0 / queueElements);
                client.DownloadProgressChanged += (sender, args) =>
                {
                    downloadProgress.Report((float)args.BytesReceived / args.TotalBytesToReceive);
                };

                var filePath = Path.Combine(downloadDirPath, $"Framework.{x++}.exe");
                await client.DownloadFileTaskAsync(downloadLink.Url, filePath);
                if (token.IsCancellationRequested)
                    throw new TaskCanceledException();

                installQueue.Enqueue(new UrlCommandlinePair()
                {
                    Parameters = downloadLink.Parameters,
                    Url = filePath
                });
            }
        }

        await Task.Run(InstallAllRuntimesAsync, token);
        async Task InstallAllRuntimesAsync()
        {
            var installSlicer = new ProgressSlicer(installSlice);
            var queueElements = urlSet.Count;

            loopstart:
            while (installQueue.TryDequeue(out UrlCommandlinePair runtimeInstallPath))
            {
                var installProgress = installSlicer.Slice(1.0 / queueElements);
                ExecuteAsAdmin(runtimeInstallPath.Url, runtimeInstallPath.Parameters);
                installProgress.Report(1);
            }

            if (!downloadTask.IsCompleted)
            {
                await Task.Delay(100);
                goto loopstart;
            }
        }

        progress.Report(1);
    }

    private static void ExecuteAsAdmin(string filePath, IEnumerable<string> arguments)
    {
        Process proc = new Process();
        filePath = Path.GetFullPath(filePath);
        proc.StartInfo.FileName = filePath;
        proc.StartInfo.SetArguments(arguments);
        proc.StartInfo.UseShellExecute = true;
        proc.StartInfo.Verb = "runas";
        proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath)!;
        proc.Start();
        proc.WaitForExit();
    }

    private static void SetArguments(this ProcessStartInfo processStartInfo, IEnumerable<string> args)
    {
        processStartInfo.Arguments = String.Join(" ", args.Select(arg => 
        {
            if (arg.Contains('"')) 
                arg = arg.Replace("\"", "\"\"");

            if (arg.Contains(' ')) 
                arg = '"' + arg + '"';

            return arg;
        }));
    }
}

public class UrlCommandlinePair
{
    /// <summary>
    /// [Optional]
    /// Friendly name for the installation.
    /// </summary>
    public string FriendlyName;

    /// <summary>
    /// URL for the runtime to be downloaded, or for the extracted runtime.
    /// </summary>
    public string Url;

    /// <summary>
    /// Commandline parameters to install the runtime components.
    /// </summary>
    public IEnumerable<string> Parameters;
}