using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml.Serialization;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Converters.NuGet.Structures;

namespace Reloaded.Mod.Loader.Update.Converters.NuGet
{
    /// <summary>
    /// Converts a packaged mod zip into a NuGet Package.
    /// </summary>
    public class Converter
    {
        private const string NugetContentDirectory = "content";

        /// <summary>
        /// Converts a mod archive (zip) into a NuGet package.
        /// </summary>
        /// <returns>The location of the newly created package.</returns>
        public string ConvertFromArchiveFile(string archivePath, string outputDirectory)
        {
            var archiveFile   = new ArchiveFile(archivePath);
            var modConfig     = JsonSerializer.Deserialize<ModConfig>(archiveFile.ExtractModConfig());
            SetNullPropertyValues(modConfig);

            // Create output directories.
            var directory = GetDirectory;
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            var contentDirectory = $"{directory}\\{NugetContentDirectory}";
            Directory.CreateDirectory(contentDirectory);

            // Extract
            archiveFile.ExtractToDirectory(contentDirectory);
            var nugetPackageOutput = FromModDirectory(contentDirectory, outputDirectory, modConfig);
            Directory.Delete(directory, true);
            return nugetPackageOutput;
        }

        /// <summary>
        /// Creates a NuGet package given the directory of a mod.
        /// </summary>
        /// <param name="modDirectory">Full path to the directory containing the mod.</param>
        /// <param name="outputDirectory">The path to the folder where the NuGet package should be output.</param>
        /// <param name="modConfig">The mod configuration for which to create the NuGet package.</param>
        /// <returns>The path of the generated .nupkg file.</returns>
        public string FromModDirectory(string modDirectory, string outputDirectory, IModConfig modConfig)
        {
            var xmlSerializer = new XmlSerializer(typeof(Package), "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd");

            // Write .nuspec
            using (TextWriter writer = new StreamWriter($"{modDirectory}\\{modConfig.ModId}.nuspec"))
            {
                xmlSerializer.Serialize(writer, FromModConfig(modConfig));
            }

            // Compress
            string nupkgPath = Path.Combine(outputDirectory, $"{modConfig.ModId}.nupkg");
            ArchiveFile.CompressDirectory(modDirectory, nupkgPath);
            return nupkgPath;
        }

        /// <summary>
        /// Generates a nuget package spec package from a given mod configuration file.
        /// </summary>
        private static Package FromModConfig(IModConfig modConfig)
        {
            var dependencies = modConfig.ModDependencies.Select(x => new Dependency(x, "0.0.0")).ToArray();
            var dependencyGroup = new DependencyGroup(dependencies);
            var metadata = new Metadata(modConfig.ModId, modConfig.ModVersion, modConfig.ModAuthor, modConfig.ModDescription, dependencyGroup);
            return new Package(metadata);
        }

        private string GetDirectory => $"{Path.GetTempPath()}\\{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}";
        private void SetNullPropertyValues(object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(obj, null);
                if (propertyValue == null)
                {
                    property.SetValue(obj,
                        property.PropertyType.IsArray
                            ? Activator.CreateInstance(property.PropertyType, 0)
                            : Activator.CreateInstance(property.PropertyType));
                }
            }
        }
    }
}
