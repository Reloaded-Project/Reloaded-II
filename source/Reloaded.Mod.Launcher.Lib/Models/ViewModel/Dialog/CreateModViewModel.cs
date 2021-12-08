using System;
using System.IO;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs;

/// <summary>
/// ViewModel used for creating an individual modification using an individual mod id.
/// </summary>
public class CreateModViewModel : ObservableObject
{
    /// <summary>
    /// Current ID of the mod.
    /// </summary>
    public string ModId { get; set; } = "";

    private readonly ModConfigService _modConfigService;

    /// <inheritdoc />
    public CreateModViewModel(ModConfigService modConfigService)
    {
        _modConfigService = modConfigService;
    }

    /// <summary>
    /// Creates the mod.
    /// </summary>
    /// <param name="showNonUniqueWindow">Shows a message to tell the user the mod isn't unique.</param>
    public async Task<PathTuple<ModConfig>> CreateMod(Action showNonUniqueWindow)
    {
        if (!IsUnique(showNonUniqueWindow))
            return null!;

        var config = new ModConfig() { ModId = ModId };
        var modDirectory = Path.Combine(IoC.Get<LoaderConfig>().ModConfigDirectory, IOEx.ForceValidFilePath(ModId));
        var filePath = Path.Combine(modDirectory, ModConfig.ConfigFileName);
        await IConfig<ModConfig>.ToPathAsync(config, filePath);
        return new PathTuple<ModConfig>(filePath, config);
    }

    /// <summary>
    /// Returns true if the mod id is unique, else false.
    /// </summary>
    /// <param name="showNonUniqueWindow">Shows a message to tell the user the mod isn't unique.</param>
    private bool IsUnique(Action showNonUniqueWindow)
    {
        if (!_modConfigService.ItemsById.ContainsKey(ModId)) 
            return true;

        showNonUniqueWindow();
        return false;
    }
}