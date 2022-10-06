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
    public ObservableCollection<BooleanGenericTuple<ApplicationConfig>> EnabledAppIds { get; set; } = new ObservableCollection<BooleanGenericTuple<ApplicationConfig>>();
    
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

        var supportedAppIds = newModTuple.Config.SupportedAppId;
        var tuples = _appConfigService.Items.Select(x => new BooleanGenericTuple<ApplicationConfig>(supportedAppIds.Contains(x.Config.AppId), x.Config));
        EnabledAppIds = new ObservableCollection<BooleanGenericTuple<ApplicationConfig>>(tuples);
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