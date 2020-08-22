using System.IO;
using System.Linq;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Exceptions;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Reloaded.Mod.Loader.Update.Converters.NuGet
{
    /// <summary>
    /// Abstracts a compressed archive for reading.
    /// </summary>
    public class ArchiveFile
    {
        private const string ConfigFileName = ModConfig.ConfigFileName;
        private static ExtractionOptions _options = new ExtractionOptions()
        {
            ExtractFullPath = true,
            Overwrite = true
        };

        private string _archivePath;
        private byte[] _archiveFile;

        public ArchiveFile(string archivePath)
        {
            _archivePath = archivePath;
            _archiveFile = File.ReadAllBytes(_archivePath);
        }

        /// <summary>
        /// Extracts the mod configuration file from the archive.
        /// </summary>
        public byte[] ExtractModConfig()
        {
            using (Stream memoryStream = new MemoryStream(_archiveFile))
            using (var factory = ArchiveFactory.Open(memoryStream))
            {
                var entry = factory.Entries.First(x => !x.IsDirectory && x.Key == ConfigFileName);
                if (entry != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    using (var entryStream = entry.OpenEntryStream())
                    {
                        entryStream.CopyTo(memory);
                        return memory.ToArray();
                    }
                }
            }

            throw new BadArchiveException($"{ConfigFileName} was not found in root of archive. Does your archive have a folder? All files should be at the root!");
        }

        /// <summary>
        /// Extracts the archive to a given temp directory.
        /// </summary>
        /// <returns>Folder where the archive has been extracted.</returns>
        public void ExtractToDirectory(string directory)
        {
            using (Stream memoryStream = new MemoryStream(_archiveFile))
            using (var factory = ArchiveFactory.Open(memoryStream))
            {
                factory.ExtractAllEntries().WriteAllToDirectory(directory, _options);
            }
        }

        /// <summary>
        /// Compresses a given folder and writes it to a specified path.
        /// </summary>
        /// <param name="directoryPath">The full path of the folder to compress.</param>
        /// <param name="outputPath">The path where the output file should be copied to.</param>
        public static void CompressDirectory(string directoryPath, string outputPath)
        {
            using (var archive = ZipArchive.Create())
            {
                archive.AddAllFromDirectory(directoryPath);
                archive.SaveTo(outputPath, CompressionType.Deflate);
            }
        }
    }
}
