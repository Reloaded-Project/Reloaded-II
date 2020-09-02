using System;
using System.Collections.Generic;
using System.Linq;
using NetCoreInstallChecker;
using NetCoreInstallChecker.Structs;
using NetCoreInstallChecker.Structs.Config;
using NetCoreInstallChecker.Structs.Config.Enum;
using RedistributableChecker;
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

        private const string CoreLoaderVersion = "3.1.0";
        private const string CoreLauncherVersion = "5.0.0";

        public DependencyChecker(bool is64Bit)
        {
            var deps = new List<IDependency>();

            deps.Add(new NetCoreDependency($".NET Core {CoreLoaderVersion} x86", ResolveLoaderCore(false)));
            deps.Add(new RedistributableDependency("Visual C++ Redistributable x86", RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x86), false));

            if (is64Bit)
            {
                deps.Add(new NetCoreDependency($".NET Core {CoreLoaderVersion} x64", ResolveLoaderCore(true)));
                deps.Add(new RedistributableDependency("Visual C++ Redistributable x64", RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x64), true));
                deps.Add(new NetCoreDependency($"[Launcher] .NET Core {CoreLauncherVersion} x64", ResolveLauncherCore()));
            }

            Dependencies = deps.ToArray();
        }

        /// <summary>
        /// Resolves .NET Core dependencies for the loader.
        /// </summary>
        private DependencySearchResult<FrameworkOptionsTuple> ResolveLoaderCore(bool is64Bit)
        {
            var framework = new Framework("Microsoft.WindowsDesktop.App", CoreLoaderVersion);
            var options   = new RuntimeOptions("netcoreapp3.1", framework, RollForwardPolicy.Minor);
            return ResolveCore(options, is64Bit);
        }

        /// <summary>
        /// Resolves .NET Core dependencies for the launcher.
        /// </summary>
        private DependencySearchResult<FrameworkOptionsTuple> ResolveLauncherCore()
        {
            var framework = new Framework("Microsoft.WindowsDesktop.App", CoreLauncherVersion);
            var options   = new RuntimeOptions("net5.0-windows", framework, RollForwardPolicy.Minor);
            return ResolveCore(options, true);
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
