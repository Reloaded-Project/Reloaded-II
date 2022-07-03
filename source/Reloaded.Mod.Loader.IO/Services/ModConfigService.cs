namespace Reloaded.Mod.Loader.IO.Services;

/// <summary>
/// Service which provides access to various mod configurations.
/// </summary>
public class ModConfigService : ConfigServiceBase<ModConfig>
{
    /// <summary>
    /// All mod user configs by their unique ID.
    /// </summary>
    public Dictionary<string, PathTuple<ModConfig>> ItemsById { get; private set; } = new Dictionary<string, PathTuple<ModConfig>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates the service instance given an instance of the configuration.
    /// </summary>
    /// <param name="config">Mod loader config.</param>
    /// <param name="context">Context to which background events should be synchronized.</param>
    public ModConfigService(LoaderConfig config, SynchronizationContext context = null)
    {
        this.OnAddItem += OnAddItemHandler;
        this.OnRemoveItem += OnRemoveItemHandler;

        Initialize(config.GetModConfigDirectory(), ModConfig.ConfigFileName, GetAllConfigs, context, true);
        SetItemsById();
    }

    /// <summary>
    /// Runs a full dependency check to check for mods with missing dependencies.
    /// </summary>
    public DependencyResolutionResult GetMissingDependencies()
    {
        // Get list of all mods.
        var allMods = Items.ToArray();
        var allModIds = new HashSet<string>(allMods.Length);
        foreach (var mod in allMods)
            allModIds.Add(mod.Config.ModId);

        var resolutionResult = new DependencyResolutionResult();
        foreach (var item in allMods)
        {
            var dependencyItem = new DependencyResolutionItem();
            dependencyItem.Mod = item.Config;

            // Get missing dependencies.
            foreach (var dependency in item.Config.ModDependencies)
            {
                if (!allModIds.Contains(dependency))
                    dependencyItem.Dependencies.Add(dependency);
            }
            
            // Add if any missing dependencies.
            if (dependencyItem.Dependencies.Count > 0)
                resolutionResult.Items.Add(dependencyItem);
        }

        return resolutionResult;
    }

    private void SetItemsById()
    {
        foreach (var item in Items)
            ItemsById[item.Config.ModId] = item;
    }

    private void OnRemoveItemHandler(PathTuple<ModConfig> obj) => ItemsById.Remove(obj.Config.ModId);

    private void OnAddItemHandler(PathTuple<ModConfig> obj) => ItemsById[obj.Config.ModId] = obj;

    private List<PathTuple<ModConfig>> GetAllConfigs() => ModConfig.GetAllMods(base.ConfigDirectory);
}

/// <summary>
/// The result of an individual dependency resolution operation.
/// </summary>
public struct DependencyResolutionResult
{
    /// <summary>
    /// True if all dependencies are available.
    /// </summary>
    public bool AllAvailable => Items.Count <= 0;

    /// <summary>
    /// List of missing dependencies.
    /// </summary>
    public List<DependencyResolutionItem> Items { get; set; } = new();

    public DependencyResolutionResult() { }
}

/// <summary>
/// An individual item returned by the dependency resolver.
/// </summary>
public struct DependencyResolutionItem
{
    public ModConfig Mod = null; 
    public List<string> Dependencies = new List<string>();

    public DependencyResolutionItem() { }
}