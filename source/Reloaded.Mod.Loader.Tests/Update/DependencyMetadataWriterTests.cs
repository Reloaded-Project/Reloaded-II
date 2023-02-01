namespace Reloaded.Mod.Loader.Tests.Update;

public class DependencyMetadataWriterTests : IDisposable
{
    /* Initialize/Dispose to protect existing config. */
    private TestEnvironmoent _testEnvironmoent = new TestEnvironmoent();

    public void Dispose()
    {
        _testEnvironmoent.Dispose();
    }

    [Fact]
    public async Task DependencyMetadataWriterFactory_BasicTest()
    {
        // Arrange 
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var tuple  = _testEnvironmoent.ModConfigService.ItemsById[_testEnvironmoent.TestModConfigA.ModId];
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(tuple, config);

        // Act
        bool modified = await DependencyMetadataWriterFactory.ExecuteAllAsync(_testEnvironmoent.ModConfigService, false);

        // Assert
        Assert.True(modified);

        var modifiedItem = _testEnvironmoent.ModConfigService.ItemsById[_testEnvironmoent.TestModConfigB.ModId];
        Assert.True(modifiedItem.Config.PluginData.ContainsKey(GameBananaDependencyMetadataWriter.PluginId));
    }

    [Fact]
    public void WithValidDependency_ImportsDependencyFromAnotherMod_UsingGitHub()
    {
        // Arrange 
        var config = new GitHubReleasesUpdateResolverFactory.GitHubConfig() { RepositoryName = "Sewer56", UserName = "Update.Test.Repo" };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GitHubReleasesUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act
        var gitHub = new GitHubReleasesDependencyMetadataWriter();
        gitHub.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Assert
        Assert.True(clonedOriginal.Config.PluginData.ContainsKey(GitHubReleasesDependencyMetadataWriter.PluginId));
    }

    [Fact]
    public void WithValidDependency_ImportsDependencyFromAnotherMod_UsingGameBanana()
    {
        // Arrange 
        var config           = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal   = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act
        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new []{ clonedDependency.Config });

        // Assert
        Assert.True(clonedOriginal.Config.PluginData.ContainsKey(GameBananaDependencyMetadataWriter.PluginId));
    }

    [Fact]
    public void WithRemovedDependency_RemovesDependency_UsingGameBanana()
    {
        // Arrange 
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedDependency2 = _testEnvironmoent.TestModConfigCTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency2, config);

        var clonedOriginal   = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act & Assert: 2 Insertions
        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config, clonedDependency2.Config });

        Assert.True(clonedOriginal.Config.PluginData.TryGetValue(GameBananaDependencyMetadataWriter.PluginId, out DependencyResolverMetadata<GameBananaUpdateResolverFactory.GameBananaConfig> metadata));
        Assert.Equal(2, metadata.IdToConfigMap.Count);

        // Act & Assert: Item Removal
        Singleton<GameBananaUpdateResolverFactory>.Instance.RemoveConfiguration(clonedDependency2);
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config, clonedDependency2.Config });

        Assert.True(clonedOriginal.Config.PluginData.TryGetValue(GameBananaDependencyMetadataWriter.PluginId, out metadata));
        Assert.Single(metadata.IdToConfigMap);
    }

    [Fact]
    public void WithNoDependencies_RemovesPluginData_UsingGitHub()
    {
        // Arrange 
        var config = new GitHubReleasesUpdateResolverFactory.GitHubConfig() { AssetFileName = "Deeznutz" };
        var gitHub = new GitHubReleasesDependencyMetadataWriter();
        WithNoDependencies_RemovesPluginData_Base<GitHubReleasesUpdateResolverFactory.GitHubConfig, GitHubReleasesUpdateResolverFactory>(gitHub, config, GameBananaDependencyMetadataWriter.PluginId);
    }

    [Fact]
    public void WithNoDependencies_RemovesPluginData_UsingGameBanana()
    {
        // Arrange 
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var gameBanana = new GameBananaDependencyMetadataWriter();
        WithNoDependencies_RemovesPluginData_Base<GameBananaUpdateResolverFactory.GameBananaConfig, GameBananaUpdateResolverFactory>(gameBanana, config, GameBananaDependencyMetadataWriter.PluginId);
    }
    
    private void WithNoDependencies_RemovesPluginData_Base<TConfigType, TUpdateResolverFactory>(IDependencyMetadataWriter metadataWriter, TConfigType config, string pluginId) where TUpdateResolverFactory : IUpdateResolverFactory, new()
    {
        // Arrange 
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<TUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act
        metadataWriter.Update(clonedOriginal.Config, new[] { clonedDependency.Config });
        Singleton<TUpdateResolverFactory>.Instance.RemoveConfiguration(clonedDependency); // Remove update info from dependency
        metadataWriter.Update(clonedOriginal.Config, new[] { clonedDependency.Config });  // Since dependency has no updates, should be removed from here too

        // Assert
        Assert.False(clonedOriginal.Config.PluginData.ContainsKey(pluginId));
    }

    [Fact]
    public void WithChangedDependency_ReturnsTrue_UsingGitHub()
    {
        // Arrange 
        var configOld = new GitHubReleasesUpdateResolverFactory.GitHubConfig() { RepositoryName = "OldRepoName" };
        var configNew = new GitHubReleasesUpdateResolverFactory.GitHubConfig() { RepositoryName = "NewRepoName" };
        WithChangedDependency_ReturnsTrue_Base<GitHubReleasesUpdateResolverFactory.GitHubConfig, GitHubReleasesUpdateResolverFactory>(new GitHubReleasesDependencyMetadataWriter(), configOld, configNew);
    }

    [Fact]
    public void WithChangedDependency_ReturnsTrue_UsingGameBanana()
    {
        // Arrange 
        var configOld = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var configNew = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 1337 };
        WithChangedDependency_ReturnsTrue_Base<GameBananaUpdateResolverFactory.GameBananaConfig, GameBananaUpdateResolverFactory>(new GameBananaDependencyMetadataWriter(), configOld, configNew);
    }
    
    private void WithChangedDependency_ReturnsTrue_Base<TConfigType, TUpdateResolverFactory>(IDependencyMetadataWriter metadataWriter, TConfigType configOld, TConfigType configNew) where TUpdateResolverFactory : IUpdateResolverFactory, new()
    {
        // Assert that Equals is properly implemented.
        Assert.Equal(configNew, configNew.DeepClone());

        // Arrange 
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<TUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, configOld);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act
        metadataWriter.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        Singleton<TUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, configNew);
        bool shouldUpdateFile = metadataWriter.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Assert
        Assert.True(shouldUpdateFile);
        Assert.NotEqual(configOld, configNew);
    }
}