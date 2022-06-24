using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Commands.Mod;
using Reloaded.Mod.Launcher.Lib.Models.Model.Application;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;

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
    /// Index of the currently selected mod.
    /// </summary>
    public int SelectedModIndex { get; set; } = -1;

    /// <summary>
    /// Stores the currently selected application.
    /// </summary>
    public PathTuple<ApplicationConfig> ApplicationTuple { get; set; }

    /// <summary/>
    public OpenModFolderCommand OpenModFolderCommand { get; set; } = null!;

    /// <summary/>
    public ConfigureModCommand ConfigureModCommand { get; set; } = null!;

    /// <summary/>
    public EditModCommand EditModCommand { get; set; } = null!;

    /// <summary/>
    public EditModUserConfigCommand EditModUserConfigCommand { get; set; } = null!;

    /// <summary/>
    public PublishModCommand PublishModCommand { get; set; } = null!;

    /// <summary/>
    public OpenUserConfigFolderCommand OpenUserConfigFolderCommand { get; set; } = null!;

    private ApplicationViewModel _applicationViewModel;
    private readonly ModUserConfigService _userConfigService;

    /// <inheritdoc />
    public ConfigureModsViewModel(ApplicationViewModel model, ModUserConfigService userConfigService)
    {
        ApplicationTuple = model.ApplicationTuple;
        _applicationViewModel = model;
        _userConfigService = userConfigService;

        // Wait for parent to fully initialize.
        _applicationViewModel.OnGetModsForThisApp += BuildModList;
        _applicationViewModel.OnLoadModSet += BuildModList;
        BuildModList();

        SelectedMod = AllMods.FirstOrDefault();
        this.PropertyChanged += OnSelectedModChanged;
        UpdateCommands();
    }

    /// <summary/>
    ~ConfigureModsViewModel() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _applicationViewModel.OnLoadModSet -= BuildModList;
        _applicationViewModel.OnGetModsForThisApp -= BuildModList;
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
        ApplicationTuple.Config.EnabledMods = AllMods.Where(x => x.Enabled == true).Select(x => x.Tuple.Config.ModId).ToArray();
        await ApplicationTuple.SaveAsync();
    }

    private void OnSelectedModChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedMod))
            UpdateCommands();
    }

    private void UpdateCommands()
    {
        if (SelectedMod == null) 
            return;

        OpenModFolderCommand = new OpenModFolderCommand(SelectedMod.Tuple);
        EditModCommand = new EditModCommand(SelectedMod.Tuple, null);
        PublishModCommand = new PublishModCommand(SelectedMod.Tuple);

        var userConfig = _userConfigService.ItemsById.GetValueOrDefault(SelectedMod.Tuple.Config.ModId);
        EditModUserConfigCommand = new EditModUserConfigCommand(userConfig);
        OpenUserConfigFolderCommand = new OpenUserConfigFolderCommand(userConfig);
        ConfigureModCommand = new ConfigureModCommand(SelectedMod.Tuple, userConfig);
    }
}