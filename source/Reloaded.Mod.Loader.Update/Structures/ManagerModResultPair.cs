using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Sewer56.Update;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Structures;

namespace Reloaded.Mod.Loader.Update.Structures;

/// <summary>
/// Tuple that joins together an update manager, a result of checking for updates and a mod.
/// </summary>
public class ManagerModResultPair
{
    /// <summary>
    /// The update manager for the mod.
    /// </summary>
    public UpdateManager<Empty> Manager { get; private set; }

    /// <summary>
    /// The result of checking for updates.
    /// </summary>
    public CheckForUpdatesResult Result { get; private set; }

    /// <summary>
    /// Provides access to the individual mod item.
    /// </summary>
    public PathTuple<ModConfig> ModTuple { get; private set; }

    /// <summary/>
    public ManagerModResultPair(UpdateManager<Empty> manager, CheckForUpdatesResult result, PathTuple<ModConfig> modTuple)
    {
        Manager = manager;
        Result = result;
        ModTuple = modTuple;
    }
}