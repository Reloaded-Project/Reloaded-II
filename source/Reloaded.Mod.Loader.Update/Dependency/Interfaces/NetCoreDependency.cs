using System;
using System.Collections.Generic;
using NetCoreInstallChecker.Structs;
using NetCoreInstallChecker.Structs.Config;
using NetCoreInstallChecker.Structs.Config.Enum;

namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces
{
    public class NetCoreDependency : IDependency
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool Available => Result.Available;

        /// <inheritdoc />
        public Architecture Architecture { get; }

        public DependencySearchResult<FrameworkOptionsTuple, Framework> Result { get; }

        public NetCoreDependency(string name, DependencySearchResult<FrameworkOptionsTuple, Framework> result, Architecture architecture)
        {
            Name = name;
            Result = result;
            Architecture = architecture;
        }

        /// <inheritdoc />
        public string[] GetUrls()
        {
            if (Result.Available) 
                return new[] {""};
            
            var urls = new List<string>();
            foreach (var dependency in Result.MissingDependencies)
            {
                string url;
                try { url = dependency.GetWindowsDownloadUrl(Architecture, Format.Executable); }
                catch (Exception) { url = ""; }

                urls.Add(url);
            }

            return urls.ToArray();

        }
    }
}
