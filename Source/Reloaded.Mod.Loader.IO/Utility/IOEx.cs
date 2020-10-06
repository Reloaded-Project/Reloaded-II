using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reloaded.Mod.Loader.IO.Utility
{
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

                while (File.Exists(destFilePath) && !CheckWriteAccess(destFilePath))
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
        /// Checks whether a file with a specific path can be written to.
        /// </summary>
        public static bool CheckWriteAccess(string filePath)
        {
            try
            {
                File.Open(filePath, FileMode.Open, FileAccess.Write).Dispose();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
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
}
