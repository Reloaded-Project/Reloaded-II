namespace Reloaded.Mod.Loader.Tests.Update.Resolvers;

public class GameBananaResolverFactoryTests : IDisposable
{
    private TestEnvironmoent _testEnvironmoent;

    public GameBananaResolverFactoryTests() => _testEnvironmoent = new TestEnvironmoent();

    public void Dispose() => _testEnvironmoent.Dispose();

    [Fact]
    public void Migrate_MigratesConfigIntoMainConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;
        var modDirectory = Path.GetDirectoryName(mod.Path);
        var configPath   = Path.Combine(modDirectory!, GameBananaUpdateResolverFactory.GameBananaConfig.ConfigFileName);
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { };
        IConfig<GameBananaUpdateResolverFactory.GameBananaConfig>.ToPath(config, configPath);

        // Act
        var factory = new GameBananaUpdateResolverFactory();
        factory.Migrate(mod, null);
        using var disposalHelper = new RemoveConfiguration<GameBananaUpdateResolverFactory.GameBananaConfig>(mod, factory);

        // Assert
        Assert.False(File.Exists(configPath));
        Assert.True(factory.TryGetConfigurationOrDefault(mod, out _));
    }

    [Fact]
    public void GetResolver_ReturnsNotNullWithConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;

        // Act
        var resolverFactory = new GameBananaUpdateResolverFactory();
        resolverFactory.SetConfiguration(mod, new GameBananaUpdateResolverFactory.GameBananaConfig());
        var resolver = resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>(), new CommonPackageResolverSettings()));

        // Assert
        Assert.NotNull(resolver);
    }

    [Fact]
    public void GetResolver_ReturnsNullOnNoConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;

        // Act
        var resolverFactory = new NuGetUpdateResolverFactory();
        var resolver = resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>(), new CommonPackageResolverSettings()));

        // Assert
        Assert.Null(resolver);
    }
}