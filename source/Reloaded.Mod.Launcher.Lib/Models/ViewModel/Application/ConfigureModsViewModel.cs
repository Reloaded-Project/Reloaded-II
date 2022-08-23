namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;

/// <summary>
/// ViewModel allowing for the configuration of mods to be loaded for a certain game.
/// </summary>
public class ConfigureModsViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// All mods available for the game.
    /// </summary>
    public ObservableCollection<ModEntry> AllMods { get; set; } = null!;

    /// <summary>
    /// The currently highlighted mod.
    /// </summary>
    public ModEntry? SelectedMod { get; set; }

    /// <summary>
    /// Stores the currently selected application.
    /// </summary>
    public PathTuple<ApplicationConfig> ApplicationTuple { get; set; }

    /// <summary/>
    public OpenModFolderCommand OpenModFolderCommand { get; set; } = null!;

    /// <summary/>
    public ConfigureModCommand ConfigureModCommand { get; set; } = null!;

    /// <summary/>
    public VisitModProjectUrlCommand VisitModProjectUrlCommand { get; set; } = null!;

    /// <summary/>
    public EditModCommand EditModCommand { get; set; } = null!;

    /// <summary/>
    public EditModUserConfigCommand EditModUserConfigCommand { get; set; } = null!;

    /// <summary/>
    public PublishModCommand PublishModCommand { get; set; } = null!;

    /// <summary/>
    public OpenUserConfigFolderCommand OpenUserConfigFolderCommand { get; set; } = null!;

    private ModEntry? _cachedModEntry;
    private ApplicationViewModel _applicationViewModel;
    private readonly ModUserConfigService _userConfigService;
    private CancellationTokenSource _saveToken;

    /// <inheritdoc />
    public ConfigureModsViewModel(ApplicationViewModel model, ModUserConfigService userConfigService)
    {
        ApplicationTuple = model.ApplicationTuple;
        _applicationViewModel = model;
        _userConfigService = userConfigService;
        _saveToken = new CancellationTokenSource();

        // Wait for parent to fully initialize.
        _applicationViewModel.OnGetModsForThisApp += BuildModList;
        _applicationViewModel.OnLoadModSet += BuildModList;
        BuildModList();

        SelectedMod = AllMods.FirstOrDefault();
        PropertyChanged += OnSelectedModChanged;
        UpdateCommands();
    }

    /// <summary/>
    ~ConfigureModsViewModel() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _applicationViewModel.OnLoadModSet -= BuildModList;
        _applicationViewModel.OnGetModsForThisApp -= BuildModList;
        _saveToken?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Builds the list of mods displayed to the user.
    /// </summary>
    private void BuildModList()
    {
        AllMods = new ObservableCollection<ModEntry>(GetInitialModSet(_applicationViewModel, ApplicationTuple));
        AllMods.CollectionChanged += async (_, _) =>
        {
            await SaveApplication(); // Save on reorder.
        }; 
    }

    /// <summary>
    /// Builds the initial set of mods to display in the list.
    /// </summary>
    private List<ModEntry> GetInitialModSet(ApplicationViewModel model, PathTuple<ApplicationConfig> applicationTuple)
    {
        // Note: Must put items in top to bottom load order.
        var enabledModIds   = applicationTuple.Config.EnabledMods;
        var modsForThisApp  = model.ModsForThisApp.ToArray();

        // Get dictionary of mods for this app by Mod ID
        var modDictionary  = new Dictionary<string, PathTuple<ModConfig>>();
        foreach (var mod in modsForThisApp)
            modDictionary[mod.Config.ModId] = mod;

        // Add enabled mods.
        var totalModList = new List<ModEntry>(modsForThisApp.Length);
        foreach (var enabledModId in enabledModIds)
        {
            if (modDictionary.ContainsKey(enabledModId))
                totalModList.Add(MakeSaveSubscribedModEntry(true, modDictionary[enabledModId]));
        }

        // Add disabled mods.
        var enabledModIdSet = applicationTuple.Config.EnabledMods.ToHashSet();
        var disabledMods    = modsForThisApp.Where(x => !enabledModIdSet.Contains(x.Config.ModId));
        totalModList.AddRange(disabledMods.Select(x => MakeSaveSubscribedModEntry(false, x)));
        return totalModList;
    }

    private ModEntry MakeSaveSubscribedModEntry(bool? isEnabled, PathTuple<ModConfig> item)
    {
        // Make BooleanGenericTuple that saves application on Enabled change.
        var tuple = new ModEntry(isEnabled, item);
        tuple.PropertyChanged += SaveOnEnabledPropertyChanged;
        return tuple;
    }

    // == Events ==
    private async void SaveOnEnabledPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == BooleanGenericTuple<IModConfig>.NameOfEnabled)
            await SaveApplication();
    }

    private async Task SaveApplication()
    {
        _saveToken.Cancel();
        _saveToken = new CancellationTokenSource();

        try
        {
            ApplicationTuple.Config.EnabledMods = AllMods.Where(x => x.Enabled == true).Select(x => x.Tuple.Config.ModId).ToArray();
            await ApplicationTuple.SaveAsync(_saveToken.Token);
        }
        catch (TaskCanceledException) { /* Ignored */ }
    }

    [SuppressPropertyChangedWarnings]
    private void OnSelectedModChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedMod))
            UpdateCommands();
    }

    private void UpdateCommands()
    {
        // Some operations like swapping order of 2 lists might fire a 
        // event with SelectedMod == null, then select our mod again.
        // Setting up some commands (particularly ConfigureMod) can cause lag, so let's mitigate this.
        if (SelectedMod == null || SelectedMod == _cachedModEntry) 
            return;

        OpenModFolderCommand = new OpenModFolderCommand(SelectedMod.Tuple);
        EditModCommand = new EditModCommand(SelectedMod.Tuple, null);
        PublishModCommand = new PublishModCommand(SelectedMod.Tuple);
        VisitModProjectUrlCommand = new VisitModProjectUrlCommand(SelectedMod.Tuple);

        var userConfig = _userConfigService.ItemsById.GetValueOrDefault(SelectedMod.Tuple.Config.ModId);
        EditModUserConfigCommand = new EditModUserConfigCommand(userConfig);
        OpenUserConfigFolderCommand = new OpenUserConfigFolderCommand(userConfig);
        ConfigureModCommand = new ConfigureModCommand(SelectedMod.Tuple, userConfig, ApplicationTuple);
        _cachedModEntry = SelectedMod;
    }
}