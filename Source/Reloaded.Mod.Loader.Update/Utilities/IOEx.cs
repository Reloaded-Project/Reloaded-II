using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Reloaded.Mod.Loader.Update.Utilities
{
    public static class IOEx
    {
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
