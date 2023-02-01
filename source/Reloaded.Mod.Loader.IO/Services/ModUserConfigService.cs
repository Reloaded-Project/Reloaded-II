namespace Reloaded.Mod.Loader.IO.Services;

/// <summary>
/// Service which provides access to various mod configurations.
/// </summary>
public class ModUserConfigService : ConfigServiceBase<ModUserConfig>
{
    /// <summary>
    /// All mod user configs by their unique ID.
    /// </summary>
    public Dictionary<string, PathTuple<ModUserConfig>> ItemsById { get; private set; } = new Dictionary<string, PathTuple<ModUserConfig>>(StringComparer.OrdinalIgnoreCase);

    private ModConfigService _modConfigService;

    /// <summary>
    /// Creates the service instance given an instance of the configuration.
    /// </summary>
    /// <param name="config">Mod loader config.</param>
    /// <param name="context">Context to which background events should be synchronized.</param>
    /// <param name="modConfigService">Allows to receive notifications on mods being deleted/created.</param>
    public ModUserConfigService(LoaderConfig config, ModConfigService modConfigService, SynchronizationContext context = null)
    {
        this.OnAddItem    += OnAddItemHandler;
        this.OnRemoveItem += OnRemoveItemHandler;

        Initialize(config.GetModUserConfigDirectory(), ModUserConfig.ConfigFileName, GetAllConfigs, context, true);
        SetItemsById();

        _modConfigService = modConfigService;
        modConfigService.OnAddItem    += CreateUserConfigOnNewConfigCreated;
        CreateConfigsForModsWithoutAny(context);
    }

    private void SetItemsById()
    {
        foreach (var item in Items)
            ItemsById[item.Config.ModId] = item;
    }

    private void OnRemoveItemHandler(PathTuple<ModUserConfig> obj)
    {
        ItemsById.Remove(obj.Config.ModId);
    }

    private void OnAddItemHandler(PathTuple<ModUserConfig> obj)
    {
        ItemsById[obj.Config.ModId] = obj;
    }

    private void CreateConfigsForModsWithoutAny(SynchronizationContext context)
    {
        void Execute()
        {
            foreach (var mod in _modConfigService.Items)
            {
                if (ItemsById.ContainsKey(mod.Config.ModId) || File.Exists(ModUserConfig.GetUserConfigPathForMod(mod.Config.ModId))) continue;

                CreateUserConfigOnNewConfigCreated(mod);
            }
        }

        if (context == null)
            Execute();
        else
            context.Post(Execute);
    }

    private List<PathTuple<ModUserConfig>> GetAllConfigs() => ModUserConfig.GetAllUserConfigs(base.ConfigDirectory);

    private void CreateUserConfigOnNewConfigCreated(PathTuple<ModConfig> tuple)
    {
        var filePath = ModUserConfig.GetUserConfigPathForMod(tuple.Config.ModId, ConfigDirectory);
        if (!File.Exists(filePath))
            IConfig<ModUserConfig>.ToPath(new ModUserConfig() { ModId = tuple.Config.ModId }, filePath);
    }
}