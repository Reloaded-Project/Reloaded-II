using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;
using SharpCompress.Archives.SevenZip;
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
            byte[] file = File.ReadAllBytes(sourceFilePath);
            await ExtractPackageAsync(file, destDirPath, progress, cancellationToken);
        }

        public async Task ExtractPackageAsync(byte[] file, string destDirPath, IProgress<double> progress = null, CancellationToken cancellationToken = new CancellationToken())
        {
            long totalSize = file.Length;

            using (Stream memoryStream = new MemoryStream(file))
            {
                if (SevenZipArchive.IsSevenZipFile(memoryStream))
                {
                    // 7z is not a streamable format, thus doesn't function with the ReaderFactory API
                    using (var sevenZipArchive = SevenZipArchive.Open(memoryStream))
                    {
                        var reader = sevenZipArchive.ExtractAllEntries();
                        await ExtractFromReaderAsync(reader, destDirPath, totalSize, progress);
                    }
                }
                else
                {
                    using (var factory = ReaderFactory.Open(memoryStream))
                    {
                        await ExtractFromReaderAsync(factory, destDirPath, totalSize, progress);
                    }
                }
            }
        }

        private async Task ExtractFromReaderAsync(IReader reader, string destDirPath, long totalSize, IProgress<double> progress = null)
        {
            long totalReadSize = 0;

            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    reader.WriteEntryToDirectory(destDirPath, _options);
                    totalReadSize += reader.Entry.CompressedSize;
                    await Task.Yield();
                    progress?.Report((double)totalReadSize / totalSize);
                }
            }
        }
    }
}
