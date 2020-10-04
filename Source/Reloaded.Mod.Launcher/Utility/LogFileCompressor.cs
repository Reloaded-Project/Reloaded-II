using System;
using System.IO;
using System.IO.Compression;

namespace Reloaded.Mod.Launcher.Utility
{
    public class LogFileCompressor : IDisposable
    {
        private ZipArchive  _archive;
        private FileStream  _fileStream;
        private string      _logFolder;

        /// <summary>
        /// Opens a zip archive for writing.
        /// </summary>
        /// <param name="logArchive">Path to the archive used for storing all of the logs.</param>
        public LogFileCompressor(string logArchive)
        {
            _logFolder  = Path.GetDirectoryName(logArchive);
            Directory.CreateDirectory(_logFolder);

            _fileStream = new FileStream(logArchive, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _archive    = new ZipArchive(_fileStream, ZipArchiveMode.Update);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _fileStream?.Dispose();
        }

        /// <summary>
        /// Alias for Dispose.
        /// </summary>
        public void Close() => Dispose();

        /// <summary>
        /// Adds all of the files from a given folder into the archive and deletes the original files.
        /// </summary>
        /// <param name="folderPath">The folder inside which to scan for files.</param>
        /// <param name="span">The minimum amount of time since last modified for files to be added.</param>
        /// <param name="filter">File name filter for the files to be added.</param>
        public void AddFiles(string folderPath, TimeSpan span, string filter = "*.txt")
        {
            var files = Directory.GetFiles(folderPath, filter);
            var now   = DateTime.UtcNow;

            foreach (var file in files)
            {
                var info      = new FileInfo(file);
                var timeSince = now - info.LastWriteTime;
                if (timeSince > span)
                {
                    _archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    File.Delete(file);
                }
            }
        }
    }
}
