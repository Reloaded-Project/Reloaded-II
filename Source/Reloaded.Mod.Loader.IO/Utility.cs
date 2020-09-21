using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Reloaded.Mod.Loader.IO
{
    public static class Utility
    {
        private static object _getFilesLock = new object();

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
            var files           = new List<string>();
            var localLockObject = new object();
            var partitioner     = Partitioner.Create(0, directories.Count);
            Parallel.ForEach(partitioner, (tuple, state) =>
            {
                var localFiles = new List<string>();
                for (int x = tuple.Item1; x < tuple.Item2; x++)
                    localFiles.AddRange(Directory.GetFiles(directories[x], fileName, SearchOption.TopDirectoryOnly));

                lock (localLockObject) 
                    files.AddRange(localFiles);
            });

            return files;
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
