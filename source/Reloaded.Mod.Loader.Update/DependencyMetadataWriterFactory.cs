namespace Reloaded.Mod.Loader.Update;

/// <summary>
/// Provides access to all dependency metadata writers.
/// </summary>
public static class DependencyMetadataWriterFactory
{
    /// <summary>
    /// List of all supported writers in preference order.
    /// </summary>
    public static IDependencyMetadataWriter[] All { get; private set; } =
    {
        // Listed in order of preference.
        new GameBananaDependencyMetadataWriter(),
        new GitHubReleasesDependencyMetadataWriter()
    };

    /// <summary>
    /// Executes the dependency metadata writer on all mod configurations.
    /// </summary>
    /// <returns>True if a change has been made.</returns>
    public static async Task<bool> ExecuteAllAsync(ModConfigService service, bool saveToFile = true)
    {
        var existingModTuples      = service.Items.ToArray();
        var existingConfigurations = existingModTuples.Select(x => x.Config).ToArray();
        bool result = false;

        foreach (var mod in existingModTuples)
        {
            // Fast fail.
            if (mod.Config.ModDependencies.Length <= 0)
                continue;

            // Get mod dependencies.
            var dependencies  = ModConfig.GetDependencies(mod.Config, existingConfigurations);
            bool needsWriting = false;

            // Try to update config.
            foreach (var writer in All)
            {
                if (writer.Update(mod.Config, dependencies.Configurations))
                    needsWriting = true;
            }

            // Save new config.
            if (needsWriting)
            {
                result = true;
                if (saveToFile)
                    await mod.SaveAsync();
            }
        }

        return result;
    }
}