using Reloaded.Mod.Loader.Update.Packs;
using Constants = Reloaded.Mod.Launcher.Lib.Misc.Constants;
using Environment = System.Environment;
using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib;

/// <summary>
/// Contains all information related to application startup.
/// </summary>
public static class Startup
{
    private static Dictionary<string, string> _commandLineArguments = new Dictionary<string, string>();

    /// <summary>
    /// Populates Command Lin
    /// </summary>
    /// <returns>True if the process should be shut down, else false.</returns>
    public static bool HandleCommandLineArgs()
    {
        var result = false;
        PopulateCommandLineArgs();

        // Check if Kill Process
        if (_commandLineArguments.TryGetValue(Constants.ParameterKill, out string? processId))
            KillProcessWithId(processId);

        // Check if Launch
        if (_commandLineArguments.TryGetValue(Constants.ParameterLaunch, out string? applicationToLaunch))
        {
            LaunchApplicationAndExit(applicationToLaunch);
            result = true;
        }

        // Check if Download Mod
        if (_commandLineArguments.TryGetValue(Constants.ParameterDownload, out string? downloadUrl))
        {
            DownloadModAndExit(downloadUrl);
            result = true;
        }

        // Check if Reloaded 2 Pack
        if (_commandLineArguments.TryGetValue(Constants.ParameterR2Pack, out string? r2PackLocation))
        {
            OpenPackAndExit(r2PackLocation);
            result = true;
        }

        return result;
    }

    // Excluding most likely unused code from JIT
    private static void KillProcessWithId(string processId)
    {
        var process = Process.GetProcessById(Convert.ToInt32(processId));
        process.Kill();

        ActionWrappers.SleepOnConditionWithTimeout(() => process.HasExited, 1000, 32);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LaunchApplicationAndExit(string applicationToLaunch)
    {
        // Acquire arguments
        var loaderConfig = IoC.Get<LoaderConfig>();
        loaderConfig.UpdatePaths(Paths.CurrentProgramFolder, Resources.ErrorLoaderNotFound.Get());
        IConfig<LoaderConfig>.ToPath(loaderConfig, Paths.LoaderConfigPath);

        _commandLineArguments.TryGetValue(Constants.ParameterArguments, out var arguments);
        arguments ??= "";
        applicationToLaunch = Path.GetFullPath(applicationToLaunch);
        
        var application = ApplicationConfig.GetAllApplications(loaderConfig.GetApplicationConfigDirectory()).FirstOrDefault(x => ApplicationConfig.GetAbsoluteAppLocation(x) == applicationToLaunch);
        if (application != null)
            arguments = $"{arguments} {application.Config.AppArguments}";

        // Show warning for Wine users.
        if (Shared.Environment.IsWine)
        {
            // Set up UI Resources, since they're needed for the dialog.
            if (CompatibilityDialogs.WineShowLaunchDialog())
                StartGame(applicationToLaunch, arguments);
        }
        else
        {
            StartGame(applicationToLaunch, arguments);
        }
    }

    // Excluding most likely unused code from JIT
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DownloadModAndExit(string downloadUrl)
    {
        if (downloadUrl.StartsWith($"{Constants.ReloadedProtocol}:", StringComparison.InvariantCultureIgnoreCase))
            downloadUrl = downloadUrl.Substring(Constants.ReloadedProtocol.Length + 1);

        var package = new WebDownloadablePackage(new Uri(downloadUrl), true);
        var viewModel = new DownloadPackageViewModel(package, IoC.Get<LoaderConfig>());
        _ = viewModel.StartDownloadAsync();

        Actions.ShowFetchPackageDialog(viewModel);
        Actions.DisplayMessagebox(Resources.PackageDownloaderDownloadCompleteTitle.Get(), Resources.PackageDownloaderDownloadCompleteDescription.Get(), new Actions.DisplayMessageBoxParams()
        {
            Type = Actions.MessageBoxType.Ok,
            StartupLocation = Actions.WindowStartupLocation.CenterScreen,
            Timeout = TimeSpan.FromSeconds(8)
        });
    }

    private static void OpenPackAndExit(string r2PackLocation)
    {
        var reader = new ReloadedPackReader(new FileStream(r2PackLocation, FileMode.Open));
        var config = IoC.Get<LoaderConfig>();
        Actions.ShowInstallModPackDialog(new InstallModPackDialogViewModel(reader, config, new AggregateNugetRepository(config.NuGetFeeds)));
    }

    private static void StartGame(string applicationToLaunch, string arguments)
    {
        // Launch the application.
        var launcher = ApplicationLauncher.FromLocationAndArguments(applicationToLaunch, arguments);
        launcher.Start();
    }
    
    private static void PopulateCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int index = 1; index < args.Length; index += 2)
            _commandLineArguments.Add(args[index], args[index + 1]);
    }
}