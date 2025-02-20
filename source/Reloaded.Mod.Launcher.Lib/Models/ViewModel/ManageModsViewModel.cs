namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// ViewModel for the page responsible for mod management.
/// </summary>
public class ManageModsViewModel : ObservableObject
{
    /// <summary>
    /// Stores the currently highlighted mod in the menu.
    /// </summary>
    public PathTuple<ModConfig>? SelectedModTuple { get; set; } = null!;

    /// <summary>
    /// Stores the list of enabled applications for this mod.
    /// </summary>
    public ObservableCollection<BooleanGenericTuple<IApplicationConfig>> EnabledAppIds { get; set; } = new ObservableCollection<BooleanGenericTuple<IApplicationConfig>>();
    
    /// <summary>
    /// Provides access to all available modifications.
    /// </summary>
    public ModConfigService ModConfigService { get; set; }
    
    /// <summary/>
    public EditModCommand EditModCommand { get; set; } = null!;

    /// <summary/>
    public DeleteModCommand DeleteModCommand { get; set; } = null!;

    /// <summary/>
    public PublishModCommand PublishModCommand { get; set; } = null!;
    
    /// <summary/>
    public CreateModPackCommand CreateModPackCommand { get; set; } = new();

    /* If false, events to reload mod list are not sent. */
    private ApplicationConfigService _appConfigService;
    private SetModImageCommand _setModImageCommand = null!;

    private ModConfig _configCopy = null!;
    
    /// <inheritdoc />
    public ManageModsViewModel(ApplicationConfigService appConfigService, ModConfigService modConfigService)
    {
        ModConfigService = modConfigService;
        _appConfigService = appConfigService;

        SelectedModTuple   = ModConfigService.Items.FirstOrDefault()!;
        CloneCurrentItem();
        PropertyChanged += OnSelectedModChanged;
        UpdateCommands();
    }

    /// <summary>
    /// Saves the old mod tuple about to be swapped out by the UI and updates the UI
    /// with the details of the new tuple.
    /// </summary>
    public void SetNewMod(PathTuple<ModConfig>? oldModTuple, PathTuple<ModConfig>? newModTuple)
    {
        // Save old collection.
        if (oldModTuple != null)
        {
            oldModTuple.Config.SupportedAppId = EnabledAppIds.Where(x => x.Enabled).Select(x => x.Generic.AppId).ToArray();
            if (!oldModTuple.Config.Equals(_configCopy) && oldModTuple.Config.ModId == _configCopy.ModId && ModConfigService.ItemsById.ContainsKey(oldModTuple.Config.ModId))
                SaveMod(oldModTuple);
        }

        // Make new collection.
        if (newModTuple == null) 
            return;

        // Available apps.
        var apps = new ObservableCollection<BooleanGenericTuple<IApplicationConfig>>();

        // Add known apps.
        foreach (var app in _appConfigService.Items)
        {
            bool isAppEnabled = newModTuple.Config.SupportedAppId.Contains(app.Config.AppId, StringComparer.OrdinalIgnoreCase);
            apps.Add(new BooleanGenericTuple<IApplicationConfig>(isAppEnabled, app.Config));
        }

        // Add unknown apps from mods.
        foreach (var mod in ModConfigService.Items)
        {
            foreach (var appId in mod.Config.SupportedAppId)
            {
                if (!apps.Any(x => x.Generic.AppId.Equals(appId, StringComparison.OrdinalIgnoreCase)))
                {
                    bool isAppEnabled = newModTuple.Config.SupportedAppId.Contains(appId, StringComparer.OrdinalIgnoreCase);
                    apps.Add(new BooleanGenericTuple<IApplicationConfig>(isAppEnabled, new UnknownApplicationConfig(appId)));
                }
            }
        }

        EnabledAppIds = apps;
        CloneCurrentItem();
    }

    /// <summary>
    /// Saves a given mod tuple to the hard disk.
    /// </summary>
    public void SaveMod(PathTuple<ModConfig>? oldModTuple)
    {
        if (oldModTuple == null) 
            return;

        oldModTuple.SaveAsync();
    }
    
    /// <summary>
    /// Filters an individual item.
    /// Returns true if the item should pass by the filter, else false.
    /// </summary>
    public bool FilterApp(string filter, BooleanGenericTuple<IApplicationConfig> item)
    {
        if (filter.Length <= 0)
            return true;

        var appNameResult = item.Generic.AppName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        var appIdResult = item.Generic.AppId.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        return appNameResult || appIdResult;
    }

    /// <summary>
    /// Sets a new mod image.
    /// </summary>
    public void SetNewImage()
    {
        if (_setModImageCommand.CanExecute(null))
            _setModImageCommand.Execute(null);
    }

    [SuppressPropertyChangedWarnings]
    private void OnSelectedModChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedModTuple))
            UpdateCommands();
    }

    private void UpdateCommands()
    {
        EditModCommand = new EditModCommand(SelectedModTuple, null);
        DeleteModCommand = new DeleteModCommand(SelectedModTuple);
        _setModImageCommand = new SetModImageCommand(SelectedModTuple);
        PublishModCommand = new PublishModCommand(SelectedModTuple);
    }

    private void CloneCurrentItem()
    {
        if (SelectedModTuple != null)
            _configCopy = SelectedModTuple.Config.ShallowClone();
    }
}