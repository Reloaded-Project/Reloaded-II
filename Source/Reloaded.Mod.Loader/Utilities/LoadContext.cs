using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using McMaster.NETCore.Plugins.Loader;
using NetCoreInstallChecker;
using NetCoreInstallChecker.Structs.Config;
using NetCoreInstallChecker.Structs.Config.Enum;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Mods;
using Reloaded.Mod.Loader.Utilities.Native;

namespace Reloaded.Mod.Loader.Utilities
{
    public class LoadContext : IDisposable
    {
        private const string DllExtension = ".dll";

        /// <summary>
        /// The context itself.
        /// </summary>
        public AssemblyLoadContext Context { get; private set; }
        
        /// <summary>
        /// Path of the default assembly assigned to this context.
        /// </summary>
        public string DefaultAssemblyPath  { get; private set; }

        private static Task _findProbingPaths;
        private static string[] _additionalProbingPaths;

        static LoadContext()
        {
            _findProbingPaths = Task.Run(() => _additionalProbingPaths = GetAdditionalProbingPaths());
        }

        public LoadContext(AssemblyLoadContext context, string defaultAssemblyPath)
        {
            Context = context;
            DefaultAssemblyPath = defaultAssemblyPath;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Context.IsCollectible)
                Context.Unload();
        }

        /// <summary>
        /// Dummy method to trigger static constructor used to preload probing paths asynchronously.
        /// </summary>
        public static void Preload() { }

        /// <summary>
        /// Loads the default assembly assigned to this <see cref="AssemblyLoadContext"/>
        /// </summary>
        public Assembly LoadDefaultAssembly() => Context.LoadFromAssemblyPath(DefaultAssemblyPath);

        /// <summary>
        /// Creates a new empty Shared <see cref="LoadContext"/> used for storing plugin shared interfaces.
        /// </summary>
        public static LoadContext BuildSharedLoadContext()
        {
            var loaderFolder      = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var interFacesAsmName = typeof(IModLoader).Assembly.GetName();
            var interFacesAsmFile = Path.Combine(loaderFolder, $"{interFacesAsmName.Name}.dll");
            var builder = new AssemblyLoadContextBuilder()
                .EnableUnloading()
                .IsLazyLoaded(true)
                .PreferDefaultLoadContextAssembly(interFacesAsmName)
                .SetMainAssemblyPath(interFacesAsmFile);

            var context = builder.Build();
            return new LoadContext(context, interFacesAsmFile);
        }

        /// <summary>
        /// Creates a new <see cref="LoadContext"/> for individual mods/plugins.
        /// </summary>
        public static LoadContext BuildModLoadContext(string assemblyPath, bool isUnloadable, Type[] sharedTypes, AssemblyLoadContext defaultContext = null)
        {
            var builder = new AssemblyLoadContextBuilder()
                .SetMainAssemblyPath(assemblyPath)
                .IsLazyLoaded(true);

            if (defaultContext != null)
                builder.SetDefaultContext(defaultContext);

            if (isUnloadable)
                builder.EnableUnloading();

            foreach (var type in sharedTypes)
                builder.PreferDefaultLoadContextAssembly(type.Assembly.GetName());

            WaitUntilFoundProbingPaths();
            foreach (var probingPath in _additionalProbingPaths)
                builder.AddProbingPath(probingPath);

            var context = builder.Build();
            return new LoadContext(context, assemblyPath);
        }

        /// <summary>
        /// Gets additional directories inside which to look for dependencies
        /// </summary>
        private static string[] GetAdditionalProbingPaths()
        {
            var additionalProbingPaths = new HashSet<string>();
            var finder                 = new FrameworkFinder(IntPtr.Size == 8);
            var dependencyResolver     = new DependencyResolver(finder);
            var runtimeOptions         = GetRuntimeOptionsForCurrentDll();

            foreach (var framework in finder.GetFrameworks())
            {
                var options         = new RuntimeOptions(runtimeOptions.Tfm, new Framework(framework, runtimeOptions.Framework.Version), runtimeOptions.RollForward);
                var resolverResults = dependencyResolver.Resolve(options);
                foreach (var dependency in resolverResults.Dependencies)
                    additionalProbingPaths.Add(dependency.FolderPath);
            }

            // The goal here is to isolate ASP & Desktop
            var netcoreApp   = EnumExtensions.ToString(FrameworkName.App);
            var orderedPaths = additionalProbingPaths.Where(x => !x.Contains(netcoreApp));
            return orderedPaths.ToArray();
        }

        /// <summary>
        /// Attempts to get the runtime options for the current mod loader DLL.
        /// </summary>
        /// <returns>Options if succeeded, else throws.</returns>
        private static RuntimeOptions GetRuntimeOptionsForCurrentDll()
        {
            var path = Path.ChangeExtension(typeof(PluginManager).Assembly.Location, "runtimeconfig.json");
            if (!File.Exists(path))
                return new RuntimeOptions("", new Framework(EnumExtensions.ToString(FrameworkName.App), Environment.Version.ToString()), RollForwardPolicy.Minor);
            
            return RuntimeOptions.FromFile(path);
        }

        /// <summary>
        /// Waits until all probing paths have been found.
        /// </summary>
        private static void WaitUntilFoundProbingPaths()
        {
            _findProbingPaths.Wait();
        }
    }
}
