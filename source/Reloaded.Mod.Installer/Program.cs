using ConsoleProgressBar;

namespace Reloaded.Mod.Installer;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Handle special case of dependency only install.
        if (args.Any(s => s.Equals("--dependenciesOnly", StringComparison.OrdinalIgnoreCase)))
        {
            InstallDependenciesOnly();
            return;
        }

        var application = new App();
        application.InitializeComponent();
        application.Run();
    }

    private static void InstallDependenciesOnly()
    {
        using var progressBar = new ProgressBar();
        var progress = progressBar.HierarchicalProgress;

        var model = new MainWindowViewModel();
        model.PropertyChanged += (sender, eventArgs) =>
        {
            if (eventArgs.PropertyName == nameof(model.Progress))
                progress.Report(model.Progress / 100.0f, "Installing Dependencies Only");
        };

        using var temporaryFolder = new TemporaryFolderAllocation();
        Console.WriteLine($"Using Temporary Folder: {temporaryFolder.FolderPath}");
        Task.Run(() => model.InstallReloadedAsync(temporaryFolder.FolderPath, false, false)).Wait();
    }
}