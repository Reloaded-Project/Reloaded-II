namespace Reloaded.Mod.Loader.Update.Tests.Providers.GameBanana;

public class GameBananaDependencyResolverTests : IDisposable
{
    /* Initialize/Dispose to protect existing config. */
    private TestEnvironmoent _testEnvironmoent = new TestEnvironmoent();

    public void Dispose()
    {
        _testEnvironmoent.Dispose();
    }

    [Fact]
    public async Task ResolveAsync_WithExistingPackage_ReturnsPackages()
    {
        // Arrange 
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 6969 };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal   = _testEnvironmoent.TestModConfigATuple.DeepClone();
        
        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Act
        var resolver = new GameBananaDependencyResolver();
        var result = await resolver.ResolveAsync(clonedDependency.Config.ModId, clonedOriginal.Config.PluginData);

        // Assert
        Assert.NotEmpty(result.FoundDependencies);
        Assert.Empty(result.NotFoundDependencies);
    }

    [Fact]
    public async Task ResolveAsync_WithExistingPackage_CanDownloadPackage()
    {
        // Arrange 
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig() { ItemId = 333681 };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        clonedDependency.Config.ReleaseMetadataFileName = "IntegrationTest.ReleaseMetadata.json";
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Act
        using var outputDirectory = new TemporaryFolderAllocation();
        var resolver = new GameBananaDependencyResolver();
        var result   = await resolver.ResolveAsync(clonedDependency.Config.ModId, clonedOriginal.Config.PluginData);
        var downloadedPackagePath = await result.FoundDependencies[0].DownloadAsync(outputDirectory.FolderPath, null);

        // Assert
        Assert.True(Directory.Exists(downloadedPackagePath));
        Assert.True(File.Exists(Path.Combine(downloadedPackagePath, ModConfig.ConfigFileName)));
    }

    [Fact]
    public async Task ResolveAsync_WithNoPackage_ReturnsMissing()
    {
        // Arrange
        var resolver = new GameBananaDependencyResolver();

        // Act
        var result = await resolver.ResolveAsync("this.package.does.not.exist");

        // Assert
        Assert.NotEmpty(result.NotFoundDependencies);
        Assert.Empty(result.FoundDependencies);
    }

    [Fact]
    public async Task ResolveAsync_WithInvalidConfig_ReturnsError()
    {
        // Arrange
        var config = new GameBananaUpdateResolverFactory.GameBananaConfig()
        {
            ItemId = -1, // Invalid item ID that should cause InitializeAsync to fail
            ItemType = "Mod"
        };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();
        
        var gameBanana = new GameBananaDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, [clonedDependency.Config]);

        // Act
        var resolver = new GameBananaDependencyResolver();
        var result = await resolver.ResolveAsync(clonedDependency.Config.ModId, clonedOriginal.Config.PluginData);

        // Assert
        Assert.Single(result.Errors);
        Assert.Equal(clonedDependency.Config.ModId, result.Errors[0].PackageId);
        Assert.Equal(nameof(GameBananaDependencyResolver), result.Errors[0].Resolver);
        Assert.NotNull(result.Errors[0].Exception);
        Assert.Contains(clonedDependency.Config.ModId, result.NotFoundDependencies);
        Assert.Empty(result.FoundDependencies);
    }
}