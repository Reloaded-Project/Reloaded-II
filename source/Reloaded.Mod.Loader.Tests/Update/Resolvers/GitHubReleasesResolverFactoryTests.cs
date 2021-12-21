using System;
using System.Collections.Generic;
using System.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.SETUP;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Resolvers;
using Reloaded.Mod.Loader.Update.Structures;
using Sewer56.Update.Structures;
using Xunit;

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
        var configPath = Path.Combine(modDirectory!, GitHubReleasesResolverFactory.GitHubConfig.ConfigFileName);
        var config = new GitHubReleasesResolverFactory.GitHubConfig() { };
        IConfig<GitHubReleasesResolverFactory.GitHubConfig>.ToPath(config, configPath);

        // Act
        var factory = new GitHubReleasesResolverFactory();
        factory.Migrate(mod, null);
        using var disposalHelper = new RemoveConfiguration<GitHubReleasesResolverFactory.GitHubConfig>(mod, factory);

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
        var configPath = Path.Combine(modDirectory!, GitHubReleasesResolverFactory.GitHubUserConfig.ConfigFileName);
        var config = new GitHubReleasesResolverFactory.GitHubUserConfig() { };
        IConfig<GitHubReleasesResolverFactory.GitHubUserConfig>.ToPath(config, configPath);

        // Act
        var factory = new GitHubReleasesResolverFactory();
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
        var resolverFactory = new GitHubReleasesResolverFactory();
        resolverFactory.SetConfiguration(mod, new GitHubReleasesResolverFactory.GitHubConfig());
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
        var resolverFactory = new NuGetResolverFactory();
        var resolver = resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>(), new CommonPackageResolverSettings()));

        // Assert
        Assert.Null(resolver);
    }
}