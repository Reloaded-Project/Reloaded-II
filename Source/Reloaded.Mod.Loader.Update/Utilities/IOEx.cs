using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.Update.Utilities
{
    public static class IOEx
    {
        /// <summary>
        /// Gets a list of all the files contained within a specific directory.
        /// </summary>
        /// <param name="directory">The absolute path of the directory from which to load all configurations from.</param>
        /// <param name="fileName">The name of the file to load. The filename can contain wildcards * but not regex.</param>
        /// <param name="maxDepth">Maximum depth to search in with 1 indicating only current directory.</param>
        public static List<string> GetFilesEx(string directory, string fileName, int maxDepth = 1)
        {
            return Utility.GetFilesEx(directory, fileName, maxDepth);
        }

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
    }
}
