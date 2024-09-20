using System;
using System.Threading.Tasks;
using ConsoleProgressBar;
using Reloaded.Mod.Installer.DependencyInstaller.IO;
using Reloaded.Mod.Installer.Lib;

namespace Reloaded.Mod.Installer.Cli;

public static class Cli
{
    public static Settings Settings;
    
    /// <summary/>
    /// <returns>True if application was executed in CLI. Else false, it should be ran in GUI.</returns>
    public static bool TryRunCli(string[] args)
    {
        // Note: This code is kind of jank, mostly hackily put together. Sorry!
        //       I was in a rush.
        Settings = Settings.GetSettings(args);
        
        // Handle special case of dependency only install.
        foreach (var arg in args)
        {
            if (arg.Equals("--dependenciesOnly", StringComparison.OrdinalIgnoreCase))
            {
                InstallDependenciesOnly();
                return true;
            }

            if (arg.Equals("--nogui"))
            {
                InstallInCli();
                return true;
            }
        }

        return false;
    }

    private static void InstallDependenciesOnly()
    {
        var model = new MainWindowViewModel();
        using var progressBar = SetupCliInstall("Installing (Dependencies Only)", model);
        using var temporaryFolder = new TemporaryFolderAllocation();
        Console.WriteLine($"Using Temporary Folder: {temporaryFolder.FolderPath}");
        Settings.InstallLocation = temporaryFolder.FolderPath; // Ensure Legacy Behaviour
        Settings.CreateShortcut = false;
        Settings.StartReloaded = false;
        Task.Run(() => model.InstallReloadedAsync(Settings)).Wait();
    }

    internal static void InstallInCli()
    {
        var model = new MainWindowViewModel();
        using var progressBar = SetupCliInstall("Installing (No GUI)", model);
        Task.Run(() => model.InstallReloadedAsync(Settings)).Wait();
    }

    private static ProgressBar SetupCliInstall(string progressText, MainWindowViewModel model)
    {
        var progressBar = new ProgressBar();
        var progress = progressBar.HierarchicalProgress;
        model = new MainWindowViewModel();
        model.PropertyChanged += (_, eventArgs) =>
        {
            if (eventArgs.PropertyName == nameof(model.Progress))
                progress.Report(model.Progress / 100.0f, progressText);
        };

        return progressBar;
    }
}