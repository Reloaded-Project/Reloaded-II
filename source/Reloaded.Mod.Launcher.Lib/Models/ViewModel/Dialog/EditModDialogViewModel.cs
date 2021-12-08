using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Commands.Mod;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// The ViewModel for a dialog which allows us to edit the details of an individual mod.
/// </summary>
public class EditModDialogViewModel : Loader.IO.Utility.ObservableObject
{
    /// <summary>
    /// The individual mod configuration to be edited.
    /// </summary>
    public ModConfig Config { get; set; }

    /// <summary>
    /// The individual mod configuration to be edited.
    /// </summary>
    public PathTuple<ModConfig> ConfigTuple { get; set; }

    /// <summary>
    /// All possible dependencies for the mod configurations.
    /// </summary>
    public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();

    /// <summary>
    /// Filter allowing for dependencies to be filtered out.
    /// </summary>
    public string ModsFilter { get; set; } = "";
    
    private readonly ApplicationConfigService _applicationConfigService;
    private SetModImageCommand _setModImageCommand;

    /// <inheritdoc />
    public EditModDialogViewModel(PathTuple<ModConfig> modTuple, ApplicationConfigService applicationConfigService, ModConfigService modConfigService)
    {
        _applicationConfigService = applicationConfigService;
        ConfigTuple = modTuple;
        Config = modTuple.Config;

        /* Build Dependencies */
        var mods = modConfigService.Items; // In case collection changes during window open.
        foreach (var mod in mods)
        {
            bool isEnabled = modTuple.Config.ModDependencies.Contains(mod.Config.ModId, StringComparer.OrdinalIgnoreCase);
            Dependencies.Add(new BooleanGenericTuple<IModConfig>(isEnabled, mod.Config));
        }

        _setModImageCommand = new SetModImageCommand(modTuple);
    }
    
    /// <summary>
    /// Saves the mod back to the mod directory.
    /// </summary>
    public void Save()
    {
        // Make folder path and save folder.
        string modDirectory = Path.GetDirectoryName(ConfigTuple.Path)!;

        // Save Config
        string configSavePath  = Path.Combine(modDirectory, ModConfig.ConfigFileName);
        Config.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();

        ConfigReader<ModConfig>.WriteConfiguration(configSavePath, (ModConfig) Config);
    }

    /* Get Image To Display */

    /// <summary>
    /// Filters an individual item.
    /// Returns true if the item should pass by the filter, else false.
    /// </summary>
    public bool FilterItem(BooleanGenericTuple<IModConfig> item)
    {
        if (ModsFilter.Length <= 0)
            return true;
        
        return item.Generic.ModName.IndexOf(this.ModsFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }

    /// <summary>
    /// Sets a new mod image.
    /// </summary>
    public void SetNewImage()
    {
        if (_setModImageCommand.CanExecute(null))
            _setModImageCommand.Execute(null);
    }
}