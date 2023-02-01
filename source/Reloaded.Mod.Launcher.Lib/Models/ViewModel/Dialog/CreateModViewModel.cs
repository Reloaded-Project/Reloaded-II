namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

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
    public async Task<PathTuple<ModConfig>?> CreateMod(Action showNonUniqueWindow)
    {
        if (!IsUnique(showNonUniqueWindow))
            return null;

        var config = new ModConfig()
        {
            ModId = ModId,
            ReleaseMetadataFileName = $"{ModId}.ReleaseMetadata.json"
        };

        var modDirectory = Path.Combine(IoC.Get<LoaderConfig>().GetModConfigDirectory(), IOEx.ForceValidFilePath(ModId));
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