using System;
using System.Threading.Tasks;
using Force.DeepCloner;
using Reloaded.Mod.Interfaces.Utilities;
using Reloaded.Mod.Loader.Tests.SETUP;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Sewer56.Update.Misc;
using Xunit;

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
    public void GameBanana_WithValidDependency_ImportsDependencyFromAnotherMod()
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
    public void GameBanana_WithRemovedDependency_RemovesDependency()
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

        Assert.True(clonedOriginal.Config.PluginData.TryGetValue(GameBananaDependencyMetadataWriter.PluginId, out GameBananaDependencyMetadataWriter.GameBananaResolverMetadata metadata));
        Assert.Equal(2, metadata.IdToConfigMap.Count);

        // Act & Assert: Item Removal
        Singleton<GameBananaUpdateResolverFactory>.Instance.RemoveConfiguration(clonedDependency2);
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config, clonedDependency2.Config });

        Assert.True(clonedOriginal.Config.PluginData.TryGetValue(GameBananaDependencyMetadataWriter.PluginId, out metadata));
        Assert.Single(metadata.IdToConfigMap);
    }

    [Fact]
    public void GameBanana_WithNoDependencies_RemovesPluginData()
    {
        // Arrange 
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act
        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        Singleton<GameBananaUpdateResolverFactory>.Instance.RemoveConfiguration(clonedDependency);
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Assert
        Assert.False(clonedOriginal.Config.PluginData.ContainsKey(GameBananaDependencyMetadataWriter.PluginId));
    }

    [Fact]
    public void GameBanana_WithChangedDependency_ReturnsTrue()
    {
        // Arrange 
        var configOld = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var configNew = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 1337 };

        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, configOld);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        // Act
        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, configNew);
        bool shouldUpdateFile = gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Assert
        Assert.True(shouldUpdateFile);
        Assert.NotEqual(configOld, configNew);
    }
}