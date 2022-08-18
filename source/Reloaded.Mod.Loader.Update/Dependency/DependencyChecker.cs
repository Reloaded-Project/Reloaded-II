namespace Reloaded.Mod.Loader.Update.Dependency;

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
    public IDependency[] Dependencies { get; private set; } = null!;

    /// <summary/>
    public DependencyChecker(string loaderPath32, string loaderPath64,  bool is64Bit)
    {
        var deps = new List<IDependency>();

        var core32 = GetRuntimeOptionsForDll(loaderPath32);
        deps.Add(new NetCoreDependency($".NET Core {core32.GetAllFrameworks()[0].Version} x86", ResolveCore(core32, false), Architecture.x86));
        deps.Add(new RedistributableDependency("Visual C++ Redistributable x86", RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x86), Architecture.x86));

        if (is64Bit)
        {
            var core64 = GetRuntimeOptionsForDll(loaderPath64);
            deps.Add(new NetCoreDependency($".NET Core {core64.GetAllFrameworks()[0].Version} x64", ResolveCore(core64, true), Architecture.Amd64));
            deps.Add(new RedistributableDependency("Visual C++ Redistributable x64", RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2015to2019x64), Architecture.Amd64));
        }

        Dependencies = deps.ToArray();
    }

    /// <summary/>
    public DependencyChecker(LoaderConfig config, bool is64Bit) : this(config.LoaderPath32, config.LoaderPath64, is64Bit) { }
    
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
    private DependencySearchResult<FrameworkOptionsTuple, Framework> ResolveCore(RuntimeOptions options, bool is64Bit)
    {
        var finder   = new FrameworkFinder(is64Bit);
        var resolver = new DependencyResolver(finder);
        return resolver.Resolve(options);
    }
}