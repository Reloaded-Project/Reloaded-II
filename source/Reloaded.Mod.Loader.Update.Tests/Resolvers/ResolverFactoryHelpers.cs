namespace Reloaded.Mod.Loader.Update.Tests.Resolvers;

public class RemoveConfiguration<T> : IDisposable
{
    public IUpdateResolverFactory Factory;
    public PathTuple<ModConfig> Config;

    public RemoveConfiguration(PathTuple<ModConfig> config, IUpdateResolverFactory factory)
    {
        Config = config;
        Factory = factory;
    }

    public void Dispose()
    {
        Factory.RemoveConfiguration(Config);
        Config.Save();
    }
}