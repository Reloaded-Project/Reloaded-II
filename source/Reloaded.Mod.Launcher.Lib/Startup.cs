using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Reloaded.Mod.Launcher.Lib.Misc;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Providers.Web;

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
        _commandLineArguments.TryGetValue(Constants.ParameterArguments, out var arguments);
        arguments ??= "";
        applicationToLaunch = Path.GetFullPath(applicationToLaunch);
        
        var application = ApplicationConfig.GetAllApplications(IoC.Get<LoaderConfig>().GetApplicationConfigDirectory()).FirstOrDefault(x => ApplicationConfig.GetAbsoluteAppLocation(x) == applicationToLaunch);
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
        viewModel.StartDownloadAsync();

        Actions.ShowFetchPackageDialog(viewModel);
        Actions.DisplayMessagebox(Resources.PackageDownloaderDownloadCompleteTitle.Get(), Resources.PackageDownloaderDownloadCompleteDescription.Get(), new Actions.DisplayMessageBoxParams()
        {
            Type = Actions.MessageBoxType.Ok,
            StartupLocation = Actions.WindowStartupLocation.CenterScreen,
            Timeout = TimeSpan.FromSeconds(8)
        });
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