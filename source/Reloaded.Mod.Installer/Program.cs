using ProgressBar = ConsoleProgressBar.ProgressBar;

namespace Reloaded.Mod.Installer;

internal class Program
{
    public static Settings Settings;
    
    [STAThread]
    public static void Main(string[] args)
    {
        // Note: This code is kind of jank, mostly hackily put together. Sorry!
        Settings = GetSettings(args);
        
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
        Settings.InstallLocation = temporaryFolder.FolderPath; // Ensure Legacy Behaviour
        Settings.CreateShortcut = false;
        Settings.StartReloaded = false;
        Task.Run(() => model.InstallReloadedAsync(Settings)).Wait();
    }

    private static void InstallNoGui()
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
        model.PropertyChanged += (sender, eventArgs) =>
        {
            if (eventArgs.PropertyName == nameof(model.Progress))
                progress.Report(model.Progress / 100.0f, progressText);
        };

        return progressBar;
    }
    
    private static Settings GetSettings(string[] args)
    {
        var settings = new Settings();
        for (int x = 0; x < args.Length - 1; x++)
        {
            if (args[x] == "--installdir") settings.InstallLocation = args[x + 1];
            if (args[x] == "--nocreateshortcut") settings.CreateShortcut = false;
            if (args[x] == "--nostartreloaded") settings.StartReloaded = false;
        }

        return settings;
    }
}