using Page = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.Page;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;

/// <summary>
/// Viewmodel allowing for editing of an individual application.
/// This ViewModel is a child/sub-page of <see cref="ApplicationViewModel"/>
/// </summary>
public class EditAppViewModel : ObservableObject
{
    /// <summary>
    /// The application being currently edited by this ViewModel.
    /// </summary>
    public PathTuple<ApplicationConfig> Application { get; set; }

    /// <summary>
    /// The service allowing for management of all application configurations.
    /// </summary>
    public ApplicationConfigService AppConfigService { get; set; }

    /// <summary>
    /// Command allowing for the deletion of an individual application.
    /// </summary>
    public CallbackCommand DeleteApplicationCommand { get; set; }

    /// <summary>
    /// Command allowing for the application image to be overwritten.
    /// </summary>
    public SetApplicationImageCommand SetApplicationImageCommand { get; set; } = null!;

    /// <summary>
    /// Command allowing for ASI Loader to be deployed to the application.
    /// </summary>
    public DeployAsiLoaderCommand DeployAsiLoaderCommand { get; set; } = null!;

    /// <summary>
    /// Allows for the current item to be saved.
    /// If false, saving is ignored.
    /// </summary>
    public bool AllowSaving { get; set; } = true;
    
    /// <summary>
    /// List of all configurable providers configurations.
    /// </summary>
    public ObservableCollection<ProviderFactoryConfiguration> PackageProviders { get; set; } = new ObservableCollection<ProviderFactoryConfiguration>();

    private PathTuple<ApplicationConfig>? _lastApplication;

    /// <inheritdoc />
    public EditAppViewModel(ApplicationConfigService appConfigService, ApplicationViewModel model)
    {
        Application = model.ApplicationTuple;
        AppConfigService = appConfigService;
        DeleteApplicationCommand = new CallbackCommand(new DeleteApplicationCommand(Application), AfterDeleteApplication);
        PropertyChanged += OnApplicationChanged;

        // Build Package Provider Configurations
        foreach (var provider in PackageProviderFactory.All)
        {
            var result = ProviderFactoryConfiguration.TryCreate(provider, Application);
            if (result != null)
                PackageProviders.Add(result);
        }

        RefreshCommands();
    }

    [SuppressPropertyChangedWarnings]
    private void OnApplicationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Application))
            RefreshCommands();
    }

    /// <summary>
    /// Switches away from deleted application after deleting it.
    /// </summary>
    private void AfterDeleteApplication()
    {
        AllowSaving = false;
        IoC.Get<MainPageViewModel>().Page = Page.SettingsPage;
    }

    /// <summary>
    /// Saves the configuration of the currently selected item.
    /// </summary>
    public async Task SaveSelectedItemAsync()
    {
        try
        {
            // Save Plugins
            foreach (var provider in PackageProviders)
            {
                if (provider.IsEnabled)
                    provider.Factory.SetConfiguration(Application, provider.Configuration);
                else
                    Application.Config.PluginData.Remove(provider.Factory.ResolverId);
            }

            if (AllowSaving)
                await Application.SaveAsync();
        }
        catch (Exception) { Debug.WriteLine($"{nameof(EditAppViewModel)}: Failed to save current selected item."); }
    }

    /// <summary>
    /// Sets the application image for the currently selected item.
    /// </summary>
    public void SetAppImage()
    {
        if (!SetApplicationImageCommand.CanExecute(null)) 
            return;

        SetApplicationImageCommand.Execute(null);
    }

    /// <summary>
    /// Used to set the new executable path for the application.
    /// </summary>
    public void SetNewExecutablePath()
    {
        var newFilePath = SelectEXEFile();
        if (string.IsNullOrEmpty(newFilePath))
            return;

        var isMsStore = TryUnprotectGamePassGame.TryIt(newFilePath);
        newFilePath = SymlinkResolver.GetFinalPathName(newFilePath);
        if (!Path.GetFileName(Application.Config.AppLocation)
                .Equals(Path.GetFileName(newFilePath), StringComparison.OrdinalIgnoreCase))
            Actions.DisplayMessagebox(Resources.AddAppWarningTitle.Get(), Resources.AddAppWarning.Get());

        // Update the AppLocation to the new executable path
        Application.Config.AppLocation = newFilePath;
        Application.Config.WorkingDirectory = CalculateNewRelativeDirectory(newFilePath);

        // Handle MS store DRM and update.
        AddApplicationCommand.HandleAddedMsStoreBinary(isMsStore, Application.Path, Application.Config);
        Application.Save();
    }

    private string CalculateNewRelativeDirectory(string newFilePath)
    {
        // Store old configuration values
        var oldWorkingDirectory = Application.Config.WorkingDirectory;
        var oldAppLocation = Application.Config.AppLocation;
        var exeParent = Path.GetDirectoryName(newFilePath)!;

        try
        {
            var relativePath = Path.GetRelativePath(oldWorkingDirectory, oldAppLocation);
            // +1 to remove final backslash.
            var newWorkingDirectory = newFilePath.Substring(0, newFilePath.Length - (relativePath.Length + 1));
            return Directory.Exists(newWorkingDirectory) 
                ? newWorkingDirectory
                : exeParent;
        }
        catch (Exception)
        {
            return exeParent;
        }
    }

    /// <summary>
    /// Allows the user to pick a repository configuration and then test it.
    /// </summary>
    public void TestRepoConfiguration()
    {
        var result = SelectJsonFile();
        if (string.IsNullOrEmpty(result))
            return;

        var item = JsonSerializer.Deserialize<AppItem>(File.ReadAllText(result));
        QueryCommunityIndexCommand.ApplyIndexEntry(item!, Application);
    }

    private void RefreshCommands()
    {
        if (_lastApplication != null)
            _lastApplication.Config.PropertyChanged -= OnAppLocationChanged;

        DeployAsiLoaderCommand     = new DeployAsiLoaderCommand(Application);
        SetApplicationImageCommand = new SetApplicationImageCommand(Application);
        _lastApplication = Application;
        _lastApplication.Config.PropertyChanged += OnAppLocationChanged;
    }

    [SuppressPropertyChangedWarnings]
    private void OnAppLocationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName!.Equals(nameof(Application.Config.AppLocation)))
            DeployAsiLoaderCommand.RaisePropertyChanged();
    }

    private string SelectEXEFile()
    {
        // This is a Save dialog because Open dialog checks read privileges,
        // and that will not work with read protected MS Store/Gamepass executables.
        var dialog = new VistaSaveFileDialog();
        dialog.OverwritePrompt = false;
        dialog.Title = Resources.AddAppExecutableTitle.Get();
        dialog.Filter = $"{Resources.AddAppExecutableFilter.Get()} (*.exe)|*.exe";
        dialog.FileName = ApplicationConfig.GetAbsoluteAppLocation(Application);

        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }


    private string SelectJsonFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.AddAppRepoTestJsonSelectTitle.Get();
        dialog.Filter = $"{Resources.AddAppRepoTestJsonSelectFilter.Get()} (*.json)|*.json";

        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }
}