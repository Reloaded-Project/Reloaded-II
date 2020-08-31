using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reloaded.Mod.Loader.IO
{
    public static class Utility
    {
        /// <summary>
        /// Gets a list of all the files contained within a specific directory.
        /// </summary>
        /// <param name="directory">The absolute path of the directory from which to load all configurations from.</param>
        /// <param name="fileName">The name of the file to load. The filename can contain wildcards * but not regex.</param>
        /// <param name="maxDepth">Maximum depth to search in with 1 indicating only current directory.</param>
        public static List<string> GetFilesEx(string directory, string fileName, int maxDepth = 1)
        {
            var files = new List<string>();
            GetFilesExInternal(directory, fileName, maxDepth, 0, files);
            return files;
        }

        private static void GetFilesExInternal(string directory, string fileName, int maxDepth, int currentDepth, List<string> files)
        {
            if (currentDepth >= maxDepth)
                return;

            files.AddRange(Directory.GetFiles(directory, fileName, SearchOption.TopDirectoryOnly));
            foreach (var subdir in Directory.GetDirectories(directory))
                GetFilesExInternal(subdir, fileName, maxDepth, currentDepth + 1, files);
        }

        /// <summary>
        /// Finds all properties which have a null values and gives them the default value for the type.
        /// </summary>
        public static void SetNullPropertyValues(object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var propertyValue = property.GetValue(obj, null);
                if (propertyValue == null)
                {
                    property.SetValue(obj,
                        property.PropertyType.IsArray
                            ? Activator.CreateInstance(property.PropertyType, 0)
                            : Activator.CreateInstance(property.PropertyType));
                }
            }
        }
    }
}
