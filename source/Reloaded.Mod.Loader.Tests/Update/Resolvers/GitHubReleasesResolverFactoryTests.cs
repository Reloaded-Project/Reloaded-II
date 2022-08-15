namespace Reloaded.Mod.Loader.Tests.Update.Resolvers;

public class GitHubReleasesResolverFactoryTests : IDisposable
{
    private TestEnvironmoent _testEnvironmoent;

    public GitHubReleasesResolverFactoryTests() => _testEnvironmoent = new TestEnvironmoent();

    public void Dispose() => _testEnvironmoent.Dispose();

    [Fact]
    public void Migrate_MigratesConfigIntoMainConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;
        var modDirectory = Path.GetDirectoryName(mod.Path);
        var configPath = Path.Combine(modDirectory!, GitHubReleasesUpdateResolverFactory.GitHubConfig.ConfigFileName);
        var config = new GitHubReleasesUpdateResolverFactory.GitHubConfig() { };
        IConfig<GitHubReleasesUpdateResolverFactory.GitHubConfig>.ToPath(config, configPath);

        // Act
        var factory = new GitHubReleasesUpdateResolverFactory();
        factory.Migrate(mod, null);
        using var disposalHelper = new RemoveConfiguration<GitHubReleasesUpdateResolverFactory.GitHubConfig>(mod, factory);

        // Assert
        Assert.False(File.Exists(configPath));
        Assert.True(factory.TryGetConfigurationOrDefault(mod, out _));
    }

    [Fact]
    public void Migrate_MigratesUserConfigIntoMainConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;
        var userConfig = _testEnvironmoent.UserConfigService.ItemsById[mod.Config.ModId];

        var modDirectory = Path.GetDirectoryName(mod.Path);
        var configPath = Path.Combine(modDirectory!, GitHubReleasesUpdateResolverFactory.GitHubUserConfig.ConfigFileName);
        var config = new GitHubReleasesUpdateResolverFactory.GitHubUserConfig() { };
        IConfig<GitHubReleasesUpdateResolverFactory.GitHubUserConfig>.ToPath(config, configPath);

        // Act
        var factory = new GitHubReleasesUpdateResolverFactory();
        factory.Migrate(mod, userConfig);

        // Assert
        Assert.False(File.Exists(configPath));
    }


    [Fact]
    public void GetResolver_ReturnsNotNullWithConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;

        // Act
        var resolverFactory = new GitHubReleasesUpdateResolverFactory();
        resolverFactory.SetConfiguration(mod, new GitHubReleasesUpdateResolverFactory.GitHubConfig());
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