using static System.Environment;
using Environment = Reloaded.Mod.Shared.Environment;
using Path = System.IO.Path;
using Paths = Reloaded.Mod.Loader.IO.Paths;

namespace Reloaded.Mod.Launcher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Entry point for the application.
    /// </summary>
    public App() => this.Startup += OnStartup;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        // Run update handler.
        if (Sewer56.Update.Hooks.Startup.HandleCommandLineArgs(GetCommandLineArgs()))
        {
            Application.Current.Shutdown(0);
            return;
        }
        
        SetupResources();

        // Common Commandline Argument Handler
        var originalMode = Application.Current.ShutdownMode;
        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (Lib.Startup.HandleCommandLineArgs())
        {
            Application.Current.Shutdown(0);
            return;
        }

        Application.Current.ShutdownMode = originalMode;
        StartProfileOptimization();
        PrepareWebRequests();

        var window = new MainWindow();
        window.ShowDialog();
    }

    private void SetupResources()
    {
        var launcherFolder= AppContext.BaseDirectory;
        var languageSelector = new XamlFileSelector($"{launcherFolder}\\Assets\\Languages");
        var themeSelector    = new XamlFileSelector($"{launcherFolder}\\Theme");
        
        var conf = Lib.IoC.GetConstant<LoaderConfig>();
        if (conf.FirstLaunch)
        {
            // Default the language to user's system language.
            // e.g. en-GB.xaml
            var currentCulture = Thread.CurrentThread.CurrentUICulture + ".xaml";
            conf.LanguageFile = languageSelector.Files.FirstOrDefault(x => Path.GetFileName(x) == currentCulture) ?? conf.LanguageFile; 
        }
        
        themeSelector.SelectXamlFileByName(Path.GetFileName(conf.ThemeFile));
        languageSelector.SelectXamlFileByName(Path.GetFileName(conf.LanguageFile));

        LibraryBindings.Init(languageSelector, themeSelector);

        // Ideally this should be in Setup, however the download dialogs should be localized.
        Resources.MergedDictionaries.Add(languageSelector);
        Resources.MergedDictionaries.Add(themeSelector);
        themeSelector.NewFileSet += OnThemeChanged;
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"{launcherFolder}\\Theme\\Helpers\\BackwardsCompatibilityHelpers.xaml", UriKind.RelativeOrAbsolute) });
    }

    private void OnThemeChanged()
    {
        void TryAssignResource(string originalResource, string targetResource)
        {
            try { Resources[targetResource] = Resources[originalResource]; }
            catch (Exception) { }
        }

        // HandyControl Compatibility
        TryAssignResource("AccentColorLighter", "DarkAccentColor");
        TryAssignResource("AccentColorLight", "SecondaryTitleColor");
        TryAssignResource("AccentColorLight", "TitleColor");
        TryAssignResource("AccentColorLighter", "DarkPrimaryColor");
        TryAssignResource("AccentColorLighter", "PrimaryColor");
        TryAssignResource("AccentColorLight", "LightPrimaryColor");

        // Remove focus from scroll viewers
        if (Current.MainWindow != null)
        {
            Current.MainWindow.ApplyTemplate();
            Current.MainWindow.OnApplyTemplate();
            Current.MainWindow.InvalidateVisual();
        }
    }

    /// <summary>
    /// Starts profile-optimization a.k.a. 'Multicore JIT'.
    /// We're not actually using this for the async JIT but to load other DLLs on a background thread to avoid an I/O bottleneck.  
    /// </summary>
    public static void StartProfileOptimization()
    {
        // Start Profile Optimization
        var profileRoot = Path.Combine(Paths.ConfigFolder, "ProfileOptimization");
        Directory.CreateDirectory(profileRoot);

        // Define the folder where to save the profile files.
        ProfileOptimization.SetProfileRoot(profileRoot);

        // Start profiling.
        ProfileOptimization.StartProfile("Launcher-startup.profile");
    }

    /// <summary>
    /// Finishes profile-optimization a.k.a. 'Multicore JIT'
    /// </summary>
    public static void StopProfileOptimization()
    {
        ProfileOptimization.StartProfile(null!);
    }
    
    private void PrepareWebRequests()
    {        
        // Raise maximum number of WebRequest connections
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        
        // When .NET makes first HTTP request, it takes some time to resolve proxy settings.
        // We can speed this up by resolving the proxy ourselves.
        Task.Run(WebRequest.GetSystemWebProxy);
    }

    /// <summary>
    /// Empties the working set of the process, purging as much memory back to RAM as possible.  
    /// We do this after initializing Reloaded (at a small perf penalty) as there's gonna be stuff that will never be needed in RAM again.
    ///
    /// We can let the CEF & Electron apps in this world (like your Slacks and Discords) eat up all the RAM instead.
    /// </summary>
    public static void EmptyWorkingSet() => EmptyWorkingSet(Environment.CurrentProcess.Handle);

    [DllImport("psapi")]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    /// <summary>
    /// Ran upon exiting the application.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        // Flush image cache on exit.
        if (Lib.IoC.IsExplicitlyBound<ImageCacheService>())
            Lib.IoC.GetConstant<ImageCacheService>().Shutdown();

        Caches.Shutdown();
    }
}