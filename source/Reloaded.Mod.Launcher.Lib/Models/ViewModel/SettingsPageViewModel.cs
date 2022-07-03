using Version = Reloaded.Mod.Launcher.Lib.Utility.Version;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// ViewModel for the Settings Page.
/// </summary>
public class SettingsPageViewModel : ObservableObject
{
    /// <summary>
    /// Provides access to all available applications.
    /// </summary>
    public ApplicationConfigService AppConfigService { get; set; }

    /// <summary>
    /// Provides access to all available mods.
    /// </summary>
    public ModConfigService ModConfigService { get; set; }

    /// <summary>
    /// Number of total applications installed.
    /// </summary>
    public int TotalApplicationsInstalled { get; set; }

    /// <summary>
    /// Number of total mods installed.
    /// </summary>
    public int TotalModsInstalled { get; set; }

    /// <summary>
    /// Copyright string to display on the Settings Panel.
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    /// The .NET Runtime version the Launcher is running on.
    /// </summary>
    public string RuntimeVersion { get; set; }

    /// <summary>
    /// Configuration for the mod loader.
    /// </summary>
    public LoaderConfig LoaderConfig { get; set; }

    /// <summary>
    /// Allows you to select the mod loader language.
    /// </summary>
    public IResourceFileSelector? LanguageSelector => Lib.LanguageSelector;

    /// <summary>
    /// Allows you to select the mod loader theme.
    /// </summary>
    public IResourceFileSelector? ThemeSelector => Lib.ThemeSelector;

    /// <summary/>
    public SettingsPageViewModel(ApplicationConfigService appConfigService, ModConfigService modConfigService, LoaderConfig loaderConfig)
    {
        AppConfigService = appConfigService;
        ModConfigService = modConfigService;
        LoaderConfig = loaderConfig;

        UpdateTotalApplicationsInstalled();
        UpdateTotalModsInstalled();
        AppConfigService.Items.CollectionChanged += MainPageViewModelOnApplicationsChanged;
        ModConfigService.Items.CollectionChanged += ManageModsViewModelOnModsChanged;

        var version = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule!.FileName!);
        Copyright = Regex.Replace(version.LegalCopyright!, @"\|.*", $"| {Version.GetReleaseVersion()!.ToNormalizedString()}");
        RuntimeVersion = $"Core: {RuntimeInformation.FrameworkDescription}";
        ActionWrappers.ExecuteWithApplicationDispatcher(() =>
        {
            SelectCurrentLanguage();
            SelectCurrentTheme();
        });
    }

    /// <summary>
    /// Asynchronously saves the loader config.
    /// </summary>
    public async Task SaveConfigAsync()
    {
        await IConfig<LoaderConfig>.ToPathAsync(LoaderConfig, Paths.LoaderConfigPath);
    }

    /// <summary>
    /// Asynchronously saves the new chosen language.
    /// </summary>
    public async Task SaveNewLanguageAsync()
    {
        if (LanguageSelector?.File != null)
        {
            LoaderConfig.LanguageFile = LanguageSelector.File;
            await SaveConfigAsync();
        }
    }

    /// <summary>
    /// Asynchronously starts saving new themes.
    /// </summary>
    public async Task SaveNewThemeAsync()
    {
        if (ThemeSelector?.File != null)
        {
            LoaderConfig.ThemeFile = ThemeSelector.File;
            await SaveConfigAsync();

            // TODO: This is a bug workaround for where the language ComboBox gets reset after a theme change.
            SelectCurrentLanguage();
        }
    }

    /// <summary>
    /// Auto selects the desired language file from a list based on path.
    /// </summary>
    public void SelectCurrentLanguage() => SelectXamlFile(LanguageSelector, LoaderConfig.LanguageFile);

    /// <summary>
    /// Auto selects the desired theme file from a list based on path.
    /// </summary>
    public void SelectCurrentTheme()    => SelectXamlFile(ThemeSelector, LoaderConfig.ThemeFile);

    /// <summary>
    /// Opens the location where the log files are stored.
    /// </summary>
    public void OpenLogFileLocation() => ProcessExtensions.OpenFileWithDefaultProgram(Paths.LogPath);
    
    /// <summary>
    /// Opens the main Reloaded configuration file.
    /// </summary>
    public void OpenConfigFile() => ProcessExtensions.OpenFileWithDefaultProgram(Paths.LoaderConfigPath);

    private void SelectXamlFile(IResourceFileSelector? selector, string fileName)
    {
        if (selector == null)
            return;

        foreach (var file in selector.Files)
        {
            if (file != fileName) 
                continue;
                
            selector.File = file;
            break;
        }
    }

    /* Functions */
    private void UpdateTotalApplicationsInstalled() => TotalApplicationsInstalled = AppConfigService.Items.Count;
    private void UpdateTotalModsInstalled() => TotalModsInstalled = ModConfigService.Items.Count;

    /* Events */
    private void ManageModsViewModelOnModsChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateTotalModsInstalled();
    private void MainPageViewModelOnApplicationsChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateTotalApplicationsInstalled();
}