using System;
using System.IO;
using System.Linq;
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
        private const string NugetPackageExtension = ".nupkg";
        private const string NugetContentDirectory = "content";

        private string _archivePath;
        private ArchiveFile _archiveFile;

        /// <summary>
        /// Converts a packaged mod archive into a NuGet Package.
        /// </summary>
        public Converter(string archivePath)
        {
            _archivePath = Path.GetFullPath(archivePath);
            _archiveFile = new ArchiveFile(_archivePath);
        }

        /// <summary>
        /// Converts the archive into a NuGet package.
        /// </summary>
        /// <returns>The location the NuGet package was extracted to.</returns>
        public string Convert()
        {
            var modConfig     = JsonSerializer.Deserialize<ModConfig>(_archiveFile.ExtractModConfig());
            var xmlSerializer = new XmlSerializer(typeof(Package), "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd");
            SetNullPropertyValues(modConfig);

            // Create output directories.
            var directory = GetDirectory;
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            var contentDirectory = $"{directory}\\{NugetContentDirectory}";
            Directory.CreateDirectory(contentDirectory);

            // Extract
            _archiveFile.ExtractToDirectory(contentDirectory);

            // Write .nuspec
            using (TextWriter writer = new StreamWriter($"{directory}\\{modConfig.ModId}.nuspec"))
            {
                xmlSerializer.Serialize(writer, FromModConfig(modConfig));
            }

            // Compress
            string nupkgPath = Path.ChangeExtension(_archivePath, NugetPackageExtension);
            _archiveFile.CompressDirectory(directory, nupkgPath);

            Directory.Delete(directory, true);
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

        private string GetDirectory => $"{Path.GetTempPath()}\\{Path.GetFileNameWithoutExtension(_archivePath)}";
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
