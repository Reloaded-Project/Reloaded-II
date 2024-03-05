namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;

/// <summary>
/// ViewModel allowing for the configuration of mods to be loaded for a certain game.
/// </summary>
public class ConfigureModsViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Special tag that includes all items.
    /// </summary>
    public const string IncludeAllTag = "! ALL TAGS !";

    /// <summary>
    /// Special tag that includes items that don't have custom code.
    /// </summary>
    public const string CodeInjectionTag = "Code Injection";

    /// <summary>
    /// Special tag that includes items that have custom code.
    /// </summary>
    public const string NoCodeInjectionTag = "No Code Injection";

    /// <summary>
    /// Special tag that includes items that use native code.
    /// </summary>
    public const string NativeModTag = "Native Mod";

    /// <summary>
    /// Special tag that excludes universal mods.
    /// </summary>
    public const string NoUniversalModsTag = "No Universal Mods";

    /// <summary>
    /// All mods available for the game.
    /// </summary>
    public ObservableCollection<ModEntry>? AllMods { get; set; } = null!;

    /// <summary>
    /// The currently highlighted mod.
    /// </summary>
    public ModEntry? SelectedMod { get; set; }

    /// <summary>
    /// Stores the currently selected application.
    /// </summary>
    public PathTuple<ApplicationConfig> ApplicationTuple { get; set; }

    /// <summary>
    /// List of all selectable tags to filter by.
    /// </summary>
    public ObservableCollection<string> AllTags { get; set; } = new ObservableCollection<string>();

    /// <summary/>
    public string SelectedTag { get; set; } = string.Empty;

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
        if (AllMods != null)
            AllMods.CollectionChanged -= OnReorderMods;
        
        var modsForThisApp = _applicationViewModel.ModsForThisApp.ToArray();
        AllMods = new ObservableCollection<ModEntry>(GetInitialModSet(modsForThisApp, ApplicationTuple));
        AllMods.CollectionChanged += OnReorderMods;
        
        Collections.ModifyObservableCollection(AllTags, GetTags(modsForThisApp).OrderBy(x => x));
        if (string.IsNullOrEmpty(SelectedTag))
            SelectedTag = AllTags[0];
    }

    private async void OnReorderMods(object? sender, NotifyCollectionChangedEventArgs e) => await SaveApplication();

    /// <summary>
    /// Builds the initial set of mods to display in the list.
    /// </summary>
    private List<ModEntry> GetInitialModSet(PathTuple<ModConfig>[] modsForThisApp, PathTuple<ApplicationConfig> applicationTuple)
    {
        // Get dictionary of mods for this app by Mod ID
        var modDictionary = new Dictionary<string, PathTuple<ModConfig>>();
        foreach (var mod in modsForThisApp)
            modDictionary[mod.Config.ModId] = mod;

        var totalModList = new List<ModEntry>(modsForThisApp.Length);

        if (applicationTuple.Config.PreserveDisabledModOrder)
        {
            // Modern Behaviour: Mod Order is Preserved
            var enabledModIds = applicationTuple.Config.EnabledMods.Where(modDictionary.ContainsKey).Distinct().ToArray();
            var sortedModIds = applicationTuple.Config.SortedMods.Where(modDictionary.ContainsKey).Distinct().ToArray();

            var enabledModIdSet = enabledModIds.ToHashSet();
            var sortedModIdSet = sortedModIds.ToHashSet();

            // Add sorted mods.
            foreach (var sortedModId in sortedModIds)
                totalModList.Add(MakeSaveSubscribedModEntry(enabledModIdSet.Contains(sortedModId), modDictionary[sortedModId]));

            // Add enabled mods that were not in the sorted mod collection.
            // This can happen in case of config upgrade from an older version.
            foreach (var enabledModId in enabledModIds.Where(x => !sortedModIdSet.Contains(x)))
                totalModList.Add(MakeSaveSubscribedModEntry(true, modDictionary[enabledModId]));

            // Add the remaining mods on the bottom of the list as disabled.
            var remainingMods = modsForThisApp.Where(x => !enabledModIdSet.Contains(x.Config.ModId) && !sortedModIdSet.Contains(x.Config.ModId)).OrderBy(x => x.Config.ModName);
            totalModList.AddRange(remainingMods.Select(x => MakeSaveSubscribedModEntry(false, x)));
        }
        else
        {
            // Classic Behaviour: Disabled Mods are Alphabetical by Name
            var enabledModIds = applicationTuple.Config.EnabledMods;

            // Add enabled mods.
            foreach (var enabledModId in enabledModIds)
            {
                if (modDictionary.ContainsKey(enabledModId))
                    totalModList.Add(MakeSaveSubscribedModEntry(true, modDictionary[enabledModId]));
            }

            // Add disabled mods.
            var enabledModIdSet = enabledModIds.ToHashSet();
            var disabledMods = modsForThisApp.Where(x => !enabledModIdSet.Contains(x.Config.ModId)).OrderBy(x => x.Config.ModName);
            totalModList.AddRange(disabledMods.Select(x => MakeSaveSubscribedModEntry(false, x)));
        }

        return totalModList;
    }

    /// <summary>
    /// Builds the initial set of mods to display in the list.
    /// </summary>
    private HashSet<string> GetTags(PathTuple<ModConfig>[] modsForThisApp)
    {
        // Note: Must put items in top to bottom load order.
        var tags = new HashSet<string>();
        tags.Add(IncludeAllTag);

        foreach (var mod in modsForThisApp)
        {
            foreach (var tag in mod.Config.Tags)
                tags.Add(tag);

            // Auto-tags
            if (mod.Config.HasDllPath())
                tags.Add(CodeInjectionTag);
            else
                tags.Add(NoCodeInjectionTag);

            if (mod.Config.IsUniversalMod)
                tags.Add(NoUniversalModsTag);

            if (mod.Config.IsNativeMod(""))
                tags.Add(NativeModTag);
        }
        return tags;
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
            // Don't update this if user doesn't want to preserve their order, in
            // case the user wants to backtrack and revert. 'e.g. I want to 'try' the other option'.
            if (ApplicationTuple.Config.PreserveDisabledModOrder)
                ApplicationTuple.Config.SortedMods = AllMods!.Select(x => x.Tuple.Config.ModId).ToArray();

            ApplicationTuple.Config.EnabledMods = AllMods!.Where(x => x.Enabled == true).Select(x => x.Tuple.Config.ModId).ToArray();
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