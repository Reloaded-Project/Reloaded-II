using Page = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.Page;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// A viewmodel for the 'main page', which consists of the sidebar and a secondary, child page on the right panel.
/// </summary>
public class MainPageViewModel : ObservableObject
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
            SelectedApplication = null;
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

    /// <summary>
    /// Switches page in a given direction.
    /// </summary>
    /// <param name="direction">The direction to switch page in. Expected values -1, 0, 1</param>
    /// <param name="sortedConfigurations">List of all sorted applications.</param>
    public void SwitchPage(int direction, IList<PathTuple<ApplicationConfig>> sortedConfigurations)
    {
        direction = NavigationUtils.NormalizeDirection(direction);

        // Get index of application
        var appIndex = sortedConfigurations.IndexOf(SelectedApplication!);
        if (appIndex == -1)
            appIndex = 0;

        // Add to current page.
        var firstAppIndex = (int)Page.Application;
        var pageCount  = firstAppIndex + sortedConfigurations.Count;
        var nextIndex = ((int)Page + appIndex + direction) % (pageCount);
        if (nextIndex < 0)
            nextIndex = pageCount - (-nextIndex); // + 1 to convert page index to page count

        // Handle regular page.
        if (nextIndex < firstAppIndex)
        {
            Page = (Page)nextIndex;
            return;
        }

        // Handle application page.
        var appOffset = nextIndex - firstAppIndex;
        SwitchToApplication(sortedConfigurations[appOffset]);
    }
}