using PropertyChanged;
using Reloaded.Mod.Launcher.Lib.Commands.Application;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// A viewmodel for the 'main page', which consists of the sidebar and a secondary, child page on the right panel.
/// </summary>
public class MainPageViewModel : Loader.IO.Utility.ObservableObject
{
    /// <summary>
    /// Stores the page to be displayed to the user.
    /// </summary>
    [DoNotNotify]
    public Page Page
    {
        get => _launcherPage;
        set
        {
            if (_launcherPage == value) 
                return;

            _launcherPage = value;
            RaisePropertyChangedEvent(nameof(Page));
        }
    }

    /// <summary>
    /// Stores the currently selected application.
    /// </summary>
    public PathTuple<ApplicationConfig>? SelectedApplication { get; private set; }
    
    /// <summary>
    /// Allows us to add an application.
    /// </summary>
    public AddApplicationCommand AddApplicationCommand      { get; private set; }
    
    /// <summary>
    /// Service providing access to all application configurations.
    /// </summary>
    public ApplicationConfigService ConfigService           { get; private set; }
    
    // Note: Do not merge with property. Used below.
    private Page _launcherPage = Page.SettingsPage;

    private AutoInjector _autoInjector;

    /// <inheritdoc />
    public MainPageViewModel(ApplicationConfigService service)
    {
        ConfigService = service;
        AddApplicationCommand = new AddApplicationCommand(this, service);
        _autoInjector = new AutoInjector(service);
    }

    /// <summary>
    /// Changes the Application page to display and displays the application.
    /// </summary>
    public void SwitchToApplication(PathTuple<ApplicationConfig> tuple)
    {
        SelectedApplication = tuple;
        _launcherPage = Page.Application;
        RaisePropertyChangedEvent(nameof(Page));
    }
}