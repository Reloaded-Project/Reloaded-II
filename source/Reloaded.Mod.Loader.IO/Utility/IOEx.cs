using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reloaded.Mod.Loader.IO.Utility;

public static class IOEx
{
    private static readonly char[] InvalidFilePathChars = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();

    /// <summary>
    /// Moves a directory from a given source path to a target path, overwriting all files.
    /// </summary>
    /// <param name="source">The source path.</param>
    /// <param name="target">The target path.</param>
    public static void MoveDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);

        // Get all files in source directory.
        var sourceFilePaths = Directory.EnumerateFiles(source);

        // Move them.
        foreach (var sourceFilePath in sourceFilePaths)
        {
            // Get destination file path
            var destFileName = Path.GetFileName(sourceFilePath);
            var destFilePath = Path.Combine(target, destFileName);

            while (File.Exists(destFilePath) && !CheckFileAccess(destFilePath, FileMode.Open, FileAccess.Write))
                Thread.Sleep(100);

            if (File.Exists(destFilePath))
                File.Delete(destFilePath);

            File.Move(sourceFilePath, destFilePath);
        }

        // Get all subdirectories in source directory.
        var sourceSubDirPaths = Directory.EnumerateDirectories(source);

        // Recursively move them.
        foreach (var sourceSubDirPath in sourceSubDirPaths)
        {
            var destSubDirName = Path.GetFileName(sourceSubDirPath);
            var destSubDirPath = Path.Combine(target, destSubDirName);
            MoveDirectory(sourceSubDirPath, destSubDirPath);
        }
    }

    /// <summary>
    /// Waits for write access to be available for a file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="mode">A mode that determines whether the file should be opened, created etc.</param>
    /// <param name="access">Required access types for the file.</param>
    /// <param name="token">The token.</param>
    public static FileStream OpenFile(string filePath, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, CancellationToken token = default)
    {
        FileStream stream;

        while ((stream = TryOpenOrCreateFileStream(filePath, mode, access)) == null) 
        {
            if (token.IsCancellationRequested)
                return null;

            Thread.Sleep(1);
        }

        return stream;
    }

    /// <summary>
    /// Waits for write access to be available for a file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="mode">A mode that determines whether the file should be opened, created etc.</param>
    /// <param name="access">Required access types for the file.</param>
    /// <param name="token">The token.</param>
    public static async Task<FileStream> OpenFileAsync(string filePath, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite, CancellationToken token = default)
    {
        FileStream stream;
        while ((stream = TryOpenOrCreateFileStream(filePath, mode, access)) == null)
        {
            if (token.IsCancellationRequested)
                return null;

            await Task.Delay(1, token);
        }

        return stream;
    }

    /// <summary>
    /// Tries to open a stream for a specified file.
    /// Returns null if it fails due to file lock.
    /// </summary>
    public static FileStream TryOpenOrCreateFileStream(string filePath, FileMode mode = FileMode.OpenOrCreate, FileAccess access = FileAccess.ReadWrite)
    {
        try
        {
            return File.Open(filePath, mode, access);
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>
    /// Waits for write access to be available for a file.
    /// </summary>
    /// <param name="source">The path to the file.</param>
    /// <param name="destination">Where the file should be moved to.</param>
    /// <param name="token">The token.</param>
    public static void MoveFile(string source, string destination, CancellationToken token = default)
    {
        while (true)
        {
            try
            {
                if (token.IsCancellationRequested)
                    return;

                File.Move(source, destination, true);
                return;
            }
            catch (Exception) { /* Ignored */ }
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// Checks whether a file with a specific path can be opened.
    /// </summary>
    public static bool CheckFileAccess(string filePath, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite)
    {
        using var stream = TryOpenOrCreateFileStream(filePath, mode, access);
        return stream != null;
    }

    /// <summary>
    /// Gets a list of all the files contained within a specific directory.
    /// </summary>
    /// <param name="directory">The absolute path of the directory from which to load all configurations from.</param>
    /// <param name="fileName">The name of the file to load. The filename can contain wildcards * but not regex.</param>
    /// <param name="maxDepth">Maximum depth (inclusive) to search in with 1 indicating only current directory.</param>
    /// <param name="minDepth">Minimum depth (inclusive) to search in with 1 indicating current directory.</param>
    public static List<string> GetFilesEx(string directory, string fileName, int maxDepth = 1, int minDepth = 1)
    {
        var directories = new List<string>();
        GetFilesExDirectories(directory, maxDepth, minDepth, 0, directories);

        // Wait for all threads to terminate.
        var files = new List<string>();
        if (directories.Count > 0) 
        {
            var localLockObject = new object();
            var partitioner = Partitioner.Create(0, directories.Count);
            Parallel.ForEach(partitioner, (tuple, state) =>
            {
                var localFiles = new List<string>();
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                    localFiles.AddRange(Directory.GetFiles(directories[x], fileName, SearchOption.TopDirectoryOnly));

                lock (localLockObject)
                    files.AddRange(localFiles);
            });
        }
            
        return files;
    }

    /// <summary>
    /// Removes invalid characters from a specified file path.
    /// </summary>
    public static string ForceValidFilePath(string text)
    {
        foreach (char c in InvalidFilePathChars)
        {
            if (c != '\\' || c != '/')
                text = text.Replace(c.ToString(), "");
        }

        return text;
    }

    /// <summary>
    /// Tries to delete a directory, if possible.
    /// </summary>
    public static void TryDeleteDirectory(string path, bool recursive = true)
    {
        try { Directory.Delete(path, recursive); }
        catch (Exception) { /* Ignored */ }
    }

    /// <summary>
    /// Tries to delete a file, if possible.
    /// </summary>
    public static void TryDeleteFile(string path)
    {
        try { File.Delete(path); }
        catch (Exception) { /* Ignored */ }
    }

    private static void GetFilesExDirectories(string directory, int maxDepth, int minDepth, int currentDepth, List<string> directories)
    {
        if (currentDepth >= minDepth - 1 && currentDepth < maxDepth)
            directories.Add(directory);

        if (currentDepth + 1 >= maxDepth) 
            return;

        foreach (var subdir in Directory.GetDirectories(directory))
            GetFilesExDirectories(subdir, maxDepth, minDepth, currentDepth + 1, directories);
    }
}