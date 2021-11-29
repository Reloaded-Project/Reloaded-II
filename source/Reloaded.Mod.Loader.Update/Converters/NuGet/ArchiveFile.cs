using System.IO;
using System.IO.Compression;
using System.Linq;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Exceptions;

namespace Reloaded.Mod.Loader.Update.Converters.NuGet
{
    /// <summary>
    /// Abstracts a compressed archive for reading.
    /// </summary>
    public class ArchiveFile
    {
        private const string ConfigFileName = ModConfig.ConfigFileName;
        private string _archivePath;

        public ArchiveFile(string archivePath)
        {
            _archivePath = archivePath;
        }

        /// <summary>
        /// Extracts the mod configuration file from the archive.
        /// </summary>
        public byte[] ExtractModConfig()
        {
            using var fileStream = File.Open(_archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var zipArchive = new ZipArchive(fileStream);

            var modConfigEntry = zipArchive.Entries.FirstOrDefault(x => x.Name == ConfigFileName);
            if (modConfigEntry == null)
                throw new BadArchiveException($"{ConfigFileName} was not found in root of archive. Does your archive have a folder? All files should be at the root!");

            using var modConfigStream = modConfigEntry.Open();
            using var memoryStream = new MemoryStream();
            modConfigStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Extracts the archive to a given temp directory.
        /// </summary>
        public void ExtractToDirectory(string directory) => ZipFile.ExtractToDirectory(_archivePath, directory, true);

        /// <summary>
        /// Compresses a given folder and writes it to a specified path.
        /// </summary>
        /// <param name="directoryPath">The full path of the folder to compress.</param>
        /// <param name="outputPath">The path where the output file should be copied to.</param>
        public static void CompressDirectory(string directoryPath, string outputPath) => ZipFile.CreateFromDirectory(directoryPath, outputPath, CompressionLevel.Optimal, false);
    }
}
