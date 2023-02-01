using CompressionLevel = System.IO.Compression.CompressionLevel;
using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility class responsible for adding and removing files to/from an archive based on last modified date.
/// </summary>
public class LogFileCompressor : IDisposable
{
    private readonly ZipArchive  _archive;
    private readonly FileStream  _fileStream;
    private readonly string      _logFolder;

    /// <summary>
    /// Opens a zip archive for writing.
    /// </summary>
    /// <param name="logArchive">Path to the archive used for storing all of the logs.</param>
    public LogFileCompressor(string logArchive)
    {
        _logFolder  = Path.GetDirectoryName(logArchive)!;
        Directory.CreateDirectory(_logFolder);

        _fileStream = new FileStream(logArchive, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        _archive    = new ZipArchive(_fileStream, ZipArchiveMode.Update);
    }

    /// <inheritdoc />
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
    /// Deletes files from the archive older than the specified amount of time (since last modified).
    /// </summary>
    /// <param name="span">The maximum amount of time to keep files. Files older will be removed.</param>
    public void DeleteOldFiles(TimeSpan span)
    {
        var now = DateTime.Now;
        foreach (var entry in _archive.Entries.ToArray())
        {
            var timeSince = now - entry.LastWriteTime;
            if (timeSince <= span) 
                continue;

            entry.Delete();
        }
    }

    /// <summary>
    /// Adds all of the files from a given folder into the archive and deletes the original files.
    /// </summary>
    /// <param name="folderPath">The folder inside which to scan for files.</param>
    /// <param name="span">The minimum amount of time since last modified for files to be added.</param>
    /// <param name="filter">File name filter for the files to be added.</param>
    public void AddFiles(string folderPath, TimeSpan span, string filter = "*.txt")
    {
        var files = Directory.GetFiles(folderPath, filter);
        var now   = DateTime.Now;

        foreach (var file in files)
        {
            var info      = new FileInfo(file);
            var timeSince = now - info.LastWriteTime;
            if (timeSince <= span) 
                continue;

            _archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            File.Delete(file);
        }
    }
}