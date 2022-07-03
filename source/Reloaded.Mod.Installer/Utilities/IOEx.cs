namespace Reloaded.Mod.Installer.Utilities
{
    // ReSharper disable once InconsistentNaming
    public static class IOEx
    {
        /// <summary>
        /// Moves a directory from a given source path to a target path, overwriting all files.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <param name="target">The target path.</param>
        public static void MoveDirectory(string source, string target)
        {
            MoveDirectory(source, target, (x, y) =>
            {
                File.Copy(x, y, true);
                File.Delete(x);
            });
        }

        /// <summary>
        /// Copies a directory from a given source path to a target path, overwriting all files.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <param name="target">The target path.</param>
        public static void CopyDirectory(string source, string target)
        {
            MoveDirectory(source, target, (x, y) => File.Copy(x, y, true));
        }

        private static void MoveDirectory(string source, string target, Action<string, string> moveDirectoryAction)
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

                moveDirectoryAction(sourceFilePath, destFilePath);
            }

            // Get all subdirectories in source directory.
            var sourceSubDirPaths = Directory.EnumerateDirectories(source);

            // Recursively move them.
            foreach (var sourceSubDirPath in sourceSubDirPaths)
            {
                var destSubDirName = Path.GetFileName(sourceSubDirPath);
                var destSubDirPath = Path.Combine(target, destSubDirName);
                MoveDirectory(sourceSubDirPath, destSubDirPath, moveDirectoryAction);
            }
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
        /// Checks whether a file with a specific path can be opened.
        /// </summary>
        public static bool CheckFileAccess(string filePath, FileMode mode = FileMode.Open, FileAccess access = FileAccess.ReadWrite)
        {
            using var stream = TryOpenOrCreateFileStream(filePath, mode, access);
            return stream != null;
        }

        /// <summary>
        /// Tries to delete a directory, if possible.
        /// </summary>
        public static void TryDeleteDirectory(string path, bool recursive = true)
        {
            try { Directory.Delete(path, recursive); }
            catch (Exception e) { /* Ignored */ }
        }

        /// <summary>
        /// Tries to delete a directory, if possible.
        /// </summary>
        public static void TryDeleteFile(string path)
        {
            try { File.Delete(path); }
            catch (Exception e) { /* Ignored */ }
        }

        /// <summary>
        /// Tries to empty a directory, if possible.
        /// </summary>
        public static void TryEmptyDirectory(string path)
        {
            TryDeleteDirectory(path, true);
            Directory.CreateDirectory(path);
        }
    }
}