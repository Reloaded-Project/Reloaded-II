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
    private static Dictionary<string, string> _commandLineArguments = new();

    /// <summary>
    /// Populates Command Lin
    /// </summary>
    /// <returns>True if the process should be shut down, else false.</returns>
    public static bool HandleCommandLineArgs()
    {
        var result = false;
        PopulateCommandLineArgs();

        // Check if Kill Process
        bool forceInject = false;
        if (_commandLineArguments.TryGetValue(Constants.ParameterKill, out string? processId))
        {
            KillProcessWithId(processId);
            // for outdated bootstrappers, assume injection is required when kill specified.
            // otherwise follow regular setting
            forceInject = true; 
        }

        // Check if Launch
        if (_commandLineArguments.TryGetValue(Constants.ParameterLaunch, out string? applicationToLaunch))
        {
            LaunchApplicationAndExit(applicationToLaunch, forceInject);
            result = true;
        }

        // Check if Download Mod
        if (_commandLineArguments.TryGetValue(Constants.ParameterDownload, out string? downloadUrl))
        {
            InitControllerSupport();
            DownloadModAndExit(downloadUrl);
            result = true;
        }

        // Check if Reloaded 2 Pack Download
        if (_commandLineArguments.TryGetValue(Constants.ParameterR2PackDownload, out string? r2PackDlUrl))
        {
            InitControllerSupport();
            DownloadAndOpenPackAndExit(r2PackDlUrl);
            result = true;
        }

        // Check if Reloaded 2 Pack
        if (_commandLineArguments.TryGetValue(Constants.ParameterR2Pack, out string? r2PackLocation))
        {
            InitControllerSupport();
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
    private static void LaunchApplicationAndExit(string applicationToLaunch, bool forceInject)
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

        _commandLineArguments.TryGetValue(Constants.ParameterWorkingDirectory, out var workingDirectory);
        var inject = !application!.Config.DontInject | forceInject;
        
        // Show warning for Wine users.
        if (Shared.Environment.IsWine)
        {
            // Set up UI Resources, since they're needed for the dialog.
            if (CompatibilityDialogs.WineShowLaunchDialog())
                StartGame(applicationToLaunch, arguments, workingDirectory, inject);
        }
        else
        {
            StartGame(applicationToLaunch, arguments, workingDirectory, inject);
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
        Update.ResolveMissingPackages();
        Actions.DisplayMessagebox(Resources.PackageDownloaderDownloadCompleteTitle.Get(), Resources.PackageDownloaderDownloadCompleteDescription.Get(), new Actions.DisplayMessageBoxParams()
        {
            Type = Actions.MessageBoxType.Ok,
            StartupLocation = Actions.WindowStartupLocation.CenterScreen,
            Timeout = TimeSpan.FromSeconds(8)
        });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DownloadAndOpenPackAndExit(string downloadUrl)
    {
        if (downloadUrl.StartsWith($"{Constants.ReloadedPackProtocol}:", StringComparison.InvariantCultureIgnoreCase))
            downloadUrl = downloadUrl.Substring(Constants.ReloadedPackProtocol.Length + 1);

        using var httpClient = new HttpClient();
        var file = new MemoryStream(Task.Run(() => httpClient.GetByteArrayAsync(downloadUrl)).Result);
        var config = IoC.Get<LoaderConfig>();
        Actions.ShowInstallModPackDialog(new InstallModPackDialogViewModel(new ReloadedPackReader(file), config, new AggregateNugetRepository(config.NuGetFeeds)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void OpenPackAndExit(string r2PackLocation)
    {
        var reader = new ReloadedPackReader(new FileStream(r2PackLocation, FileMode.Open));
        var config = IoC.Get<LoaderConfig>();
        Actions.ShowInstallModPackDialog(new InstallModPackDialogViewModel(reader, config, new AggregateNugetRepository(config.NuGetFeeds)));
    }

    private static void InitControllerSupport() => Actions.InitControllerSupport();

    private static void StartGame(string applicationToLaunch, string arguments, string? workingDirectory, bool inject)
    {
        // Launch the application.
        var launcher = ApplicationLauncher.FromLocationAndArguments(applicationToLaunch, arguments, workingDirectory);
        launcher.Start(inject);
    }
    
    private static void PopulateCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int index = 1; index < args.Length; index += 2)
            _commandLineArguments.Add(args[index], args[index + 1].Trim('"'));

        // Reason for trimming, see:
        // https://developercommunity.visualstudio.com/t/environmentgetcommandlineargs-and-myapplicationcom/473717#T-N503552
        // https://bytes.com/topic/c-sharp/answers/238413-problem-environment-getcommandlineargs
        // TLDR;
        // Ending backslashes - as commonly used to differantiate files and folders - cause Environment.GetCommandLineArgs() to escape the ending qutoation mark.
        // Just trim it off.
    }
}