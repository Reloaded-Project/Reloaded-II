using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Reloaded.Mod.Loader.Update.Extractors
{
    public class ArchiveExtractor : IPackageExtractor
    {
        private static ExtractionOptions _options = new ExtractionOptions()
        {
            ExtractFullPath = true,
            Overwrite = true
        };

        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath, IProgress<double> progress = null, CancellationToken cancellationToken = new CancellationToken())
        {
            byte[] file = File.ReadAllBytes(sourceFilePath);
            await ExtractPackageAsync(file, destDirPath, progress, cancellationToken);
        }

        public async Task ExtractPackageAsync(byte[] file, string destDirPath, IProgress<double> progress = null, CancellationToken cancellationToken = new CancellationToken())
        {
            using (Stream memoryStream = new MemoryStream(file))
            {
                memoryStream.Position = 0;
                long totalReadSize = 0;
                using (var factory = ArchiveFactory.Open(memoryStream))
                {
                    foreach (var entry in factory.Entries.Where(entry => !entry.IsDirectory))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        entry.WriteToDirectory(destDirPath, _options);
                        totalReadSize += entry.Size;
                        await Task.Yield();
                        progress?.Report((double)totalReadSize / factory.TotalUncompressSize);
                    }
                }
            }
        }
    }
}
