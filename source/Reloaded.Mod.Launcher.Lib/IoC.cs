using Container = IoC.Container;

namespace Reloaded.Mod.Launcher.Lib;

/// <summary>
/// Provides access to an inversion of control container that can be used for passing static values around.
/// </summary>
public static class IoC
{
    /// <summary>
    /// The standard IoC Container.
    /// </summary>
    public static Container Container { get; } = Container.Create("Global IoC Container");

    private static readonly Dictionary<Type, IToken> TypeToTokenMap = new Dictionary<Type, IToken>();

    /// <summary>
    /// Sets up the IoC container with any relevant bindings on first access of the class.
    /// </summary>
    static IoC()
    {
        // Setup loader configuration.
        LoaderConfig config;
        try
        {
            Paths.IsReloadedLauncher = true;
            config = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath);
        }
        catch (Exception ex)
        {
            config = new LoaderConfig();
            config.SanitizeConfig();
            Errors.HandleException(ex, "Failed to parse Reloaded-II launcher configuration.\n" +
                                       "This is a rare bug, your settings have been reset.\n" +
                                       "If you have encountered this please report this to the GitHub issue tracker.\n" +
                                       "Any information on how to reproduce this would be very, very welcome.\n");

            IConfig<LoaderConfig>.ToPath(config, Paths.LoaderConfigPath);
        }

        BindToConstant(config);
    }

    /// <summary>
    /// Retrieves a service (class) from the IoC of a specified type.
    /// </summary>
    public static T Get<T>()
    {
        if (!IsExplicitlyBound<T>())
        {
            // We need to do it this way because rebind might fail with existing binding.
            BindToTransient<T>();
            return Container.Resolve<T>();
        }
        
        return Container.Resolve<T>();
    }

    /// <summary>
    /// Binds the given type to a transient value (new type is created each time).
    /// </summary>
    /// <typeparam name="T">The type of the value to bind.</typeparam>
    public static IToken BindToTransient<T>()
    {
        var token = Container.Bind<T>().As(Lifetime.Transient).To();
        TypeToTokenMap[typeof(T)] = token;
        return token;
    }

    /// <summary>
    /// Binds the given type to a constant value.
    /// </summary>
    /// <typeparam name="T">The type of the value to bind.</typeparam>
    /// <param name="item">The item to set.</param>
    public static IToken BindToConstant<T>(T item)
    {
        var token = Container.Bind<T>().As(Lifetime.ContainerSingleton).To(context => item);
        TypeToTokenMap[typeof(T)] = token;
        return token;
    }

    /// <summary>
    /// Re-binds the given type to a constant value.
    /// Use with singleton dialogs only. Remember to unbind.
    /// </summary>
    /// <typeparam name="T">The type of the value to bind.</typeparam>
    /// <param name="item">The item to set.</param>
    public static IToken RebindToConstant<T>(T item)
    {
        Unbind<T>();
        return BindToConstant(item);
    }

    /// <summary>
    /// Unbinds a specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void Unbind<T>()
    {
        if (TypeToTokenMap.Remove(typeof(T), out var token))
            token.Dispose();
    }

    /// <summary>
    /// Unbinds a specified type.
    /// </summary>
    public static void Unbind<T>(T value) => Unbind<T>();

    /// <summary>
    /// Retrieves a constant service/class.
    /// If none is registered, binds it as the new constant to then be re-acquired.
    /// </summary>
    public static T GetConstant<T>()
    {
        // Ensure singleton is created.
        if (!IsExplicitlyBound<T>())
            Container.Bind<T>().As(Lifetime.ContainerSingleton).To();

        // Resolve our singleton (which will bind it)
        return Container.Resolve<T>(); 
    }

    /// <summary>
    /// Returns true if a type has been bound by the user, else false.
    /// </summary>
    public static bool IsExplicitlyBound<T>() => Container.IsBound<T>();
}