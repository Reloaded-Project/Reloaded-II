using System;
using System.IO;
using System.Reflection;

namespace Reloaded.Mod.Loader.Bootstrap
{
    /// <summary>
    /// Finds 
    /// </summary>
    public class LocalAssemblyResolver
    {
        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string thisAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dllInAssemblyFolder = $"{thisAssemblyFolder}\\{new AssemblyName(args.Name).Name}.dll";

            // Try loading from the current folder.
            if (File.Exists(dllInAssemblyFolder))
                return Assembly.LoadFrom(dllInAssemblyFolder);

            // Panic mode! Search all subdirectories!
            string[] libraries = Directory.GetFiles(thisAssemblyFolder, args.Name, SearchOption.AllDirectories);
            return libraries.Length > 0 ? Assembly.LoadFrom(libraries[0]) : null;
        }
    }
}
