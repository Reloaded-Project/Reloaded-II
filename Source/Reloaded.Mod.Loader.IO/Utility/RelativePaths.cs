using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Reloaded.Mod.Loader.IO.Utility
{
    /// <summary>
    /// Utility class for working with Relative paths which supports features such as copy, copy by hardlink,
    /// and get all relative paths.
    /// </summary>
    public static class RelativePaths
    {
        /// <summary>
        /// Retrieves all relative file paths to a directory.
        /// </summary>
        /// <param name="directory">Absolute path to directory to get file paths from. </param>
        public static List<string> GetRelativeFilePaths(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Select(x => x.Replace(directory, "")).ToList();
        }

        /// <summary>
        /// Builds a map of relative file paths to full paths for all files in a directory.
        /// </summary>
        /// <param name="directory">The directory to build the map from..</param>
        /// <param name="relativeDirectory">
        ///     The directory that the paths should be relative to.
        ///     If not specified, <see cref="directory"/> will be used.
        /// </param>
        public static Dictionary<string, string> BuildRelativeFileLookup(string directory, string relativeDirectory = null)
        {
            var lookup = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (relativeDirectory == null)
                relativeDirectory = directory;

            foreach (var filePath in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (TryGetRelativePath(filePath, relativeDirectory, out var relativePath))
                    lookup[relativePath] = filePath;
            }

            return lookup;
        }

        /// <summary>
        /// Tries to get a relative path of the <see cref="path"/> parameter to <see cref="directory"/>. 
        /// Returns false if the path is relative.
        /// Returns true if the path is not relative.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="directory">The folder the path should be relative to.</param>
        /// <param name="relativePath">The relative path of the file to the directory if the operation succeeded.</param>
        public static bool TryGetRelativePath(string path, string directory, out string relativePath)
        {
            string fullPath = Path.GetFullPath(path);
            string fullDirectory = Path.GetFullPath(directory);
            bool containsDirectory = fullPath.IndexOf(fullDirectory, StringComparison.OrdinalIgnoreCase) >= 0;

            if (containsDirectory)
            {
                relativePath = path.Substring(fullDirectory.Length);
                return true;
            }

            relativePath = null;
            return false;
        }

        /// <summary>
        /// Retrieves a relative path of <see cref="path"/> to the <see cref="directory"/>.
        /// Returns null if the <see cref="path"/> is not relative.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="directory">The folder the path should be relative to.</param>
        public static string GetRelativePath(string path, string directory)
        {
            TryGetRelativePath(path, directory, out var relativePath);
            return relativePath;
        }

        /// <summary>
        /// Copies files from the <see cref="sourceDirectory"/> to the <see cref="targetDirectory"/> provided a list of relative paths
        /// <see cref="RelativePaths"/> of files to copy.
        /// </summary>
        /// <param name="relativePaths">Relative paths of files to be copied.</param>
        /// <param name="sourceDirectory">Source directory to copy files from. Should not end on a back/forward slash.</param>
        /// <param name="targetDirectory">Target directory to copy files to. Should not end on a back/forward slash.</param>
        /// <param name="fileCopyMethod">Specifies the way the files will be copied from A to B.</param>
        /// <param name="overWrite">Declares whether the files should be overwritten or not.</param>
        public static void CopyByRelativePath(IEnumerable<string> relativePaths, string sourceDirectory, string targetDirectory, FileCopyMethod fileCopyMethod, bool overWrite, bool onlyNewer = false)
        {
            CopyByRelativePath(relativePaths, sourceDirectory, targetDirectory, fileCopyMethod, overWrite, onlyNewer, null);
        }

        /// <summary>
        /// Copies files from the <see cref="sourceDirectory"/> to the <see cref="targetDirectory"/> provided a list of relative paths
        /// <see cref="RelativePaths"/> of files to copy.
        /// </summary>
        /// <param name="sourceDirectory">Source directory to copy files from. Should not end on a back/forward slash.</param>
        /// <param name="targetDirectory">Target directory to copy files to. Should not end on a back/forward slash.</param>
        /// <param name="fileCopyMethod">Specifies the way the files will be copied from A to B.</param>
        /// <param name="overWrite">Declares whether the files should be overwritten or not.</param>
        /// <param name="onlyNewer">Will only copy the file if the file is newer than the existing file.</param>
        public static void CopyByRelativePath(string sourceDirectory, string targetDirectory, FileCopyMethod fileCopyMethod, bool overWrite, bool onlyNewer = false)
        {
            // Obtain the relative paths to the target directory.
            List<string> relativePaths = GetRelativeFilePaths(sourceDirectory);

            // Call the other overload.
            CopyByRelativePath(relativePaths, sourceDirectory, targetDirectory, fileCopyMethod, overWrite, onlyNewer, null);
        }


        private static void CopyByRelativePath(IEnumerable<string> relativePaths, string sourceDirectory, string targetDirectory, FileCopyMethod fileCopyMethod, bool overWrite, bool onlyNewer, object _)
        {
            // For each relative path.
            foreach (string relativePath in relativePaths)
            {
                // Get copy paths.
                string targetPath = targetDirectory + relativePath;
                string sourcePath = sourceDirectory + relativePath;

                // Perform safety checks.
                if (!File.Exists(sourcePath))
                    continue;

                if (onlyNewer && File.Exists(targetPath))
                {
                    if (File.GetLastWriteTime(sourcePath).ToUniversalTime() < File.GetLastWriteTime(targetPath).ToUniversalTime())
                        continue; 
                }

                string targetDirectoryPath = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetDirectoryPath))
                    Directory.CreateDirectory(targetDirectoryPath ?? throw new InvalidOperationException("Target directory is null."));

                // Copy the files from A to B using the specified target method.
                CopyWithMethod(sourcePath, targetPath, fileCopyMethod, overWrite);
            }
        }

        private static void CopyWithMethod(string sourcePath, string targetPath, FileCopyMethod fileCopyMethod, bool overWrite)
        {
            try
            {
                switch (fileCopyMethod)
                {
                    case FileCopyMethod.Copy:
                        File.Copy(sourcePath, targetPath, overWrite);
                        break;

                    case FileCopyMethod.Hardlink:

                        // Delete existing file/hardlink.
                        if (File.Exists(targetPath))
                            File.Delete(targetPath);

                        // If the operation fails, copy the file with replacement.
                        if (CreateHardLink(targetPath, sourcePath, IntPtr.Zero) == false)
                            File.Copy(sourcePath, targetPath, overWrite);

                        break;
                }
            }
            catch (IOException) // File already exists.
            { }
            catch (Exception ex)
            { throw new Exception($"[RelativePaths] Tried to overwrite/copy a file and failed. {targetPath}", ex); }

        }

        /// <summary>
        /// Specifies the method by which files are meant to be copied for copy operations.
        /// Files can either be literally copied with their data being copied or a hardlink.
        /// </summary>
        public enum FileCopyMethod
        {
            /// <summary>
            /// Copies the files, literally from A to B.
            /// </summary>
            Copy,

            /// <summary>
            /// Creates a hardlink from target A to destination B,
            /// both A & B still point to the same physical data on the hard disk.
            /// </summary>
            Hardlink
        }

        /// <summary>
        /// Creates a hardlink for an already existing specific file elsewhere at another path.
        /// </summary>
        /// <param name="lpFileName">The name of the new file. This parameter may include the path but cannot specify the name of a directory.</param>
        /// <param name="lpExistingFileName">The name of the existing file. This parameter may include the path cannot specify the name of a directory.</param>
        /// <param name="lpSecurityAttributes">Reserved, should be set to null (IntPtr.Zero).</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
    }
}
