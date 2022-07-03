namespace Reloaded.Mod.Loader.Update.Packaging.Converters.NuGet;

/// <summary>
/// Converts a packaged mod zip into a NuGet Package.
/// </summary>
public static class Converter
{
    /// <summary>
    /// Converts a mod archive (zip) into a NuGet package.
    /// </summary>
    /// <returns>The location of the newly created package.</returns>
    public static async Task<string> FromArchiveFileAsync(string archivePath, string outputDirectory)
    {
        using var temporaryFolder = new TemporaryFolderAllocation();
        var extractor = new NuGetPackageExtractor();
        await extractor.ExtractPackageAsync(archivePath, temporaryFolder.FolderPath);
        return await FromModDirectoryAsync(temporaryFolder.FolderPath, outputDirectory);
    }

    /// <summary>
    /// Creates a NuGet package given the directory of a mod.
    /// </summary>
    /// <param name="modDirectory">Full path to the directory containing the mod.</param>
    /// <param name="outputDirectory">The path to the folder where the NuGet package should be output.</param>
    /// <returns>The path of the generated .nupkg file.</returns>
    public static async Task<string> FromModDirectoryAsync(string modDirectory, string outputDirectory)
    {
        var configFilePath = Path.Combine(modDirectory, ModConfig.ConfigFileName);
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException($"Failed to convert folder to NuGet Package. Unable to find config at {configFilePath}");

        var config = await IConfig<ModConfig>.FromPathAsync(configFilePath);
        var result = await Publisher.PublishAsync(new Publisher.PublishArgs()
        {
            PublishTarget = Publisher.PublishTarget.NuGet,
            ModTuple = new PathTuple<ModConfig>(configFilePath, config),
            OutputFolder = outputDirectory
        });

        return Path.Combine(outputDirectory, result.Releases[0].FileName);
    }
}