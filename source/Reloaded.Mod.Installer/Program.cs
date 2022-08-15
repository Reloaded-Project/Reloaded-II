using ProgressBar = ConsoleProgressBar.ProgressBar;

namespace Reloaded.Mod.Installer;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Handle special case of dependency only install.
        foreach (var arg in args)
        {
            if (arg.Equals("--dependenciesOnly", StringComparison.OrdinalIgnoreCase))
            {
                InstallDependenciesOnly();
                return;
            }

            if (arg.Equals("--nogui"))
            {
                InstallNoGui();
                return;
            }
        }

        var application = new App();
        application.InitializeComponent();
        application.Run();
    }

    private static void InstallDependenciesOnly()
    {
        var model = new MainWindowViewModel();
        using var progressBar = SetupCliInstall("Installing (Dependencies Only)", model);
        using var temporaryFolder = new TemporaryFolderAllocation();
        Console.WriteLine($"Using Temporary Folder: {temporaryFolder.FolderPath}");
        Task.Run(() => model.InstallReloadedAsync(temporaryFolder.FolderPath, false, false)).Wait();
    }

    private static void InstallNoGui()
    {
        var model = new MainWindowViewModel();
        using var progressBar = SetupCliInstall("Installing (No GUI)", model);
        Task.Run(() => model.InstallReloadedAsync()).Wait();
    }

    private static ProgressBar SetupCliInstall(string progressText, MainWindowViewModel model)
    {
        var progressBar = new ProgressBar();
        var progress = progressBar.HierarchicalProgress;
        model = new MainWindowViewModel();
        model.PropertyChanged += (sender, eventArgs) =>
        {
            if (eventArgs.PropertyName == nameof(model.Progress))
                progress.Report(model.Progress / 100.0f, progressText);
        };

        return progressBar;
    }
}