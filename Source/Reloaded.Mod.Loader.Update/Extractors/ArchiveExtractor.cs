using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;
using SharpCompress.Common;
using SharpCompress.Readers;

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
            byte[] file     = File.ReadAllBytes(sourceFilePath);
            long totalSize  = file.Length;
            long totalReadSize = 0;

            using (Stream memoryStream = new MemoryStream(file))
            using (var factory = ReaderFactory.Open(memoryStream))
            {
                while (factory.MoveToNextEntry())
                {
                    if (!factory.Entry.IsDirectory)
                    {
                        factory.WriteEntryToDirectory(destDirPath, _options);
                        totalReadSize += factory.Entry.CompressedSize;
                        await Task.Yield();
                        progress?.Report((double) totalReadSize / totalSize);
                    }
                }
            }

        }
    }
}
