using System;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Tests.Update.Resolvers;

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