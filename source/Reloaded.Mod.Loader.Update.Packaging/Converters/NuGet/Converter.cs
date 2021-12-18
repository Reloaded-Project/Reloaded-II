using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Config;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Resolvers.NuGet;

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

        var config = JsonSerializer.Deserialize<ModConfig>(await File.ReadAllTextAsync(configFilePath));
        return await FromModDirectoryAsync(modDirectory, outputDirectory, config);
    }

    /// <summary>
    /// Creates a NuGet package given the directory of a mod.
    /// </summary>
    /// <param name="modDirectory">Full path to the directory containing the mod.</param>
    /// <param name="outputDirectory">The path to the folder where the NuGet package should be output.</param>
    /// <param name="modConfig">The mod configuration for which to create the NuGet package.</param>
    /// <returns>The path of the generated .nupkg file.</returns>
    public static async Task<string> FromModDirectoryAsync(string modDirectory, string outputDirectory, IModConfig modConfig)
    {
        modDirectory = Path.GetFullPath(modDirectory);
        var packageArchiver = new NuGetPackageArchiver(new NuGetPackageArchiverSettings()
        {
            Id = modConfig.ModId,
            Description = modConfig.ModDescription,
            Authors = new List<string>() { modConfig.ModAuthor },
        });

        var extras = new CreateArchiveExtras()
        {
            Metadata = new PackageMetadata() { Version = modConfig.ModVersion }
        };

        string nupkgPath = Path.Combine(outputDirectory, $"{modConfig.ModId}.nupkg");
        var allFiles  = Directory.GetFiles(modDirectory, "*.*", SearchOption.AllDirectories).Select(x => Paths.GetRelativePath(x, modDirectory)).ToList();
        await packageArchiver.CreateArchiveAsync(allFiles, modDirectory, nupkgPath, extras);
        return nupkgPath;
    }
}