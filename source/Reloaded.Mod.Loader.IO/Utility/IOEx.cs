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
            
            while (File.Exists(sourceFilePath) && !CheckFileAccess(sourceFilePath, FileMode.Open, FileAccess.Write))
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
    /// Moves a file, waiting infinitely until write access is available.
    /// </summary>
    /// <param name="source">The path to the file.</param>
    /// <param name="destination">Where the file should be moved to.</param>
    /// <param name="token">The token.</param>
    public static void MoveFile(string source, string destination, CancellationToken token = default)
    {
        while (true)
        {
            if (TryMoveFile(source, destination, token))
                break;

            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// Moves a file, waiting infinitely until write access is available.
    /// </summary>
    /// <param name="source">The path to the file.</param>
    /// <param name="destination">Where the file should be moved to.</param>
    /// <param name="token">The token.</param>
    public static async Task MoveFileAsync(string source, string destination, CancellationToken token = default)
    {
        while (true)
        {
            if (TryMoveFile(source, destination, token))
                break;

            await Task.Delay(1, token);
        }
    }

    /// <summary>
    /// Tries to move a file, returns true if success, else false.
    /// </summary>
    /// <param name="source">The path to the file.</param>
    /// <param name="destination">Where the file should be moved to.</param>
    /// <param name="token">The token.</param>
    public static bool TryMoveFile(string source, string destination, CancellationToken token = default)
    {
        try
        {
            if (token.IsCancellationRequested)
                return true;

            File.Move(source, destination, true);
            return true;
        }
        catch (Exception)
        {
            return false;
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
    /// <param name="recurseOnFound">Continues to search in subdirectories even if <see cref="fileName"/> is found.</param>
    public static List<string> GetFilesEx(string directory, string fileName, int maxDepth = 1, int minDepth = 1, bool recurseOnFound = true)
    {
        var files = new List<string>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            GetFilesExDirectories_Generic(directory, fileName, maxDepth, minDepth, 0, files, recurseOnFound);
        else
            GetFilesExDirectories_Windows(directory, fileName, maxDepth, minDepth, 0, files, recurseOnFound);

        return files;
    }

    /// <summary>
    /// Gets all files in a given directory, accelerated for Windows platform.
    /// </summary>
    /// <param name="path">The path to get files from.</param>
    /// <param name="fileName">Pattern to search.</param>
    public static string[] GetFilesInDirectory(string path, string fileName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Directory.GetFiles(path, fileName, SearchOption.TopDirectoryOnly);

#pragma warning disable CA1416 // Validate platform compatibility
        var files = new List<NtQueryDirectoryFileSearcher.FileInformation>();
        var directories = new List<NtQueryDirectoryFileSearcher.DirectoryInformation>();

        if (!NtQueryDirectoryFileSearcher.TryGetDirectoryContents(path, files, directories))
            return Directory.GetFiles(path, fileName, SearchOption.TopDirectoryOnly);

        var filePaths = new List<string>(files.Count);
        foreach (var file in files)
        {
            if (file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                filePaths.Add(Path.Combine(file.DirectoryPath, file.FileName));
        }
#pragma warning restore CA1416
        return filePaths.ToArray();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetFilesExDirectories_Windows(string directory, string fileName, int maxDepth, int minDepth, int currentDepth, List<string> files, bool recurseOnFound = false)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        var listFileInfo = new List<NtQueryDirectoryFileSearcher.FileInformation>();
        var listDirInfo = new List<NtQueryDirectoryFileSearcher.DirectoryInformation>();
        GetFilesExDirectories_Windows_Internal(directory, fileName, maxDepth, minDepth, currentDepth, files, listFileInfo, listDirInfo, recurseOnFound);
#pragma warning restore CA1416 // Validate platform compatibility
    }

    [SupportedOSPlatform("windows5.1.2600")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetFilesExDirectories_Windows_Internal(string directory, string fileName, int maxDepth,
        int minDepth, int currentDepth, List<string> files,
        List<NtQueryDirectoryFileSearcher.FileInformation> fileInfoBuffer,
        List<NtQueryDirectoryFileSearcher.DirectoryInformation> dirInfoBuffer, bool recurseOnFound)
    {
        var stopSearch = false;
        fileInfoBuffer.Clear();
        dirInfoBuffer.Clear();
        
        // We always need to query because we need directories.
        NtQueryDirectoryFileSearcher.TryGetDirectoryContents(directory, fileInfoBuffer, dirInfoBuffer);
        if (currentDepth >= minDepth - 1 && currentDepth < maxDepth)
        {
            foreach (var fileInfo in fileInfoBuffer)
            {
                if (fileInfo.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    stopSearch = !recurseOnFound;
                    files.Add(Path.Combine(fileInfo.DirectoryPath, fileInfo.FileName));
                }
            }
        }

        if (currentDepth + 1 >= maxDepth || stopSearch)
            return;

        foreach (var subdir in dirInfoBuffer.ToArray())
            GetFilesExDirectories_Windows_Internal(subdir.FullPath, fileName, maxDepth, minDepth, currentDepth + 1, files, fileInfoBuffer, dirInfoBuffer, recurseOnFound);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void GetFilesExDirectories_Generic(string directory, string fileName, int maxDepth, int minDepth, int currentDepth, List<string> files, bool recurseOnFound = false)
    {
        var stopSearch = false;
        if (currentDepth >= minDepth - 1 && currentDepth < maxDepth)
        {
            var foundFiles = GetFilesInDirectory(directory, fileName);
            stopSearch = foundFiles.Length > 0 && !recurseOnFound;
            files.AddRange(foundFiles);
        }

        if (currentDepth + 1 >= maxDepth || stopSearch) 
            return;

        foreach (var subdir in Directory.GetDirectories(directory))
            GetFilesExDirectories_Generic(subdir, fileName, maxDepth, minDepth, currentDepth + 1, files);
    }
}