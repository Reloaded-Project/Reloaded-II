using System.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Sewer56.Update.Packaging.Structures;

namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Miscellaneous extensions for existing items used within the library.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Retrieves verification info from a mod config and path tuple.
    /// </summary>
    /// <param name="modConfig">Config and path tuple.</param>
    public static ReleaseMetadataVerificationInfo GetVerificationInfo(this PathTuple<ModConfig> modConfig)
    {
        return new ReleaseMetadataVerificationInfo()
        {
            FolderPath = Path.GetDirectoryName(modConfig.Path)
        };
    }
}