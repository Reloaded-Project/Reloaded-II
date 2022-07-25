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
    public static T Get<T>() => Container.Resolve<T>();

    /// <summary>
    /// Binds the given type to a constant value.
    /// </summary>
    /// <typeparam name="T">The type of the value to obtain.</typeparam>
    /// <param name="item">The item to set.</param>
    public static void BindToConstant<T>(T item)
    {
        var token = Container.Bind<T>().As(Lifetime.ContainerSingleton).To(context => item);
        TypeToTokenMap[typeof(T)] = token;
    }

    /// <summary>
    /// Re-binds the given type to a constant value.
    /// </summary>
    /// <typeparam name="T">The type of the value to obtain.</typeparam>
    /// <param name="item">The item to set.</param>
    public static void RebindToConstant<T>(T item)
    {
        if (TypeToTokenMap.Remove(typeof(T), out var token))
            token.Dispose();
        
        BindToConstant(item);
    }

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