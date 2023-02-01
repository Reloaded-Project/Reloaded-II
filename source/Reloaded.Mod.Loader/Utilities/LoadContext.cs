namespace Reloaded.Mod.Loader.Utilities;

public class LoadContext : IDisposable
{
    /// <summary>
    /// The context itself.
    /// </summary>
    public AssemblyLoadContext Context { get; private set; }
        
    /// <summary>
    /// Path of the default assembly assigned to this context.
    /// </summary>
    public string DefaultAssemblyPath  { get; private set; }

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

        var context = builder.Build();
        return new LoadContext(context, assemblyPath);
    }
}