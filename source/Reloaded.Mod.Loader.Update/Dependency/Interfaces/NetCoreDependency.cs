using System;
using System.Collections.Generic;
using NetCoreInstallChecker.Structs;

namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces
{
    public class NetCoreDependency : IDependency
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool Available => Result.Available;

        public DependencySearchResult<FrameworkOptionsTuple> Result { get; }

        public NetCoreDependency(string name, DependencySearchResult<FrameworkOptionsTuple> result)
        {
            Name = name;
            Result = result;
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
                try { url = dependency.Framework.GetInstallUrl(); }
                catch (Exception) { url = ""; }

                urls.Add(url);
            }

            return urls.ToArray();

        }
    }
}
