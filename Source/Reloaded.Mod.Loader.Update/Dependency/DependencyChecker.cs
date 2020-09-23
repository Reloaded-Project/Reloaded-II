using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetCoreInstallChecker;
using NetCoreInstallChecker.Structs;
using NetCoreInstallChecker.Structs.Config;
using NetCoreInstallChecker.Structs.Config.Enum;
using RedistributableChecker;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Dependency.Interfaces;

namespace Reloaded.Mod.Loader.Update.Dependency
{
    /// <summary>
    /// Checks and stores status of all necessary dependencies for Reloaded-II.
    /// </summary>
    public class DependencyChecker
    {
        /// <summary>
        /// True if all dependencies are available.
        /// </summary>
        public bool AllAvailable => Dependencies.All(x => x.Available);

        /// <summary>
        /// List of all dependencies consumed by the project.
        /// </summary>
        public IDependency[] Dependencies { get; }

        public DependencyChecker(LoaderConfig config, bool is64Bit)
        {
            var deps = new List<IDependency>();

            var core32 = GetRuntimeOptionsForDll(config.LoaderPath32);
            deps.Add(new NetCoreDependency($".NET Core {core32.Framework.Version} x86", ResolveCore(core32, false)));
            deps.Add(new RedistributableDependency("Visual C++ Redistributable x86", RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x86), false));

            if (is64Bit)
            {
                var core64 = GetRuntimeOptionsForDll(config.LoaderPath64);
                deps.Add(new NetCoreDependency($".NET Core {core64.Framework.Version} x64", ResolveCore(core64, true)));
                deps.Add(new RedistributableDependency("Visual C++ Redistributable x64", RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x64), true));
            }

            Dependencies = deps.ToArray();
        }

        /// <summary>
        /// Attempts to get the runtime options for a DLL or EXE by finding a runtime configuration file.
        /// </summary>
        /// <param name="dllPath">Full path to a given DLL or exe.</param>
        /// <returns>Options if succeeded, else throws.</returns>
        private RuntimeOptions GetRuntimeOptionsForDll(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath))
                throw new ArgumentException("Given DLL Path is null or empty.");

            if (!File.Exists(dllPath))
                throw new ArgumentException("Given DLL does not exist.");

            var configFilePath = Path.ChangeExtension(dllPath, "runtimeconfig.json");
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Configuration file (runtimeconfig.json) not found for given DLL.");
            
            return RuntimeOptions.FromFile(configFilePath);
        }

        /// <summary>
        /// Resolves .NET Core dependencies.
        /// </summary>
        private DependencySearchResult<FrameworkOptionsTuple> ResolveCore(RuntimeOptions options, bool is64Bit)
        {
            var finder   = new FrameworkFinder(is64Bit);
            var resolver = new DependencyResolver(finder);
            return resolver.Resolve(options);
        }
    }
}
