namespace Reloaded.Mod.Loader.Tests.Update.Providers.GitHub;

public class GitHubDependencyResolverTests : IDisposable
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
        var config = new GitHubReleasesUpdateResolverFactory.GitHubConfig()
        {
            RepositoryName = "Update.Test.Repo",
            UserName = "Sewer56"
        };
        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        Singleton<GitHubReleasesUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal   = _testEnvironmoent.TestModConfigATuple.DeepClone();
        
        var gitHub = new GitHubReleasesDependencyMetadataWriter();
        gitHub.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Act
        var resolver = new GitHubDependencyResolver();
        var result = await resolver.ResolveAsync(clonedDependency.Config.ModId, clonedOriginal.Config.PluginData);

        // Assert
        Assert.NotEmpty(result.FoundDependencies);
        Assert.Empty(result.NotFoundDependencies);
    }

    [Fact]
    public async Task ResolveAsync_WithExistingPackage_CanDownloadPackage()
    {
        // Arrange
        var config = new GitHubReleasesUpdateResolverFactory.GitHubConfig()
        {
            RepositoryName = "reloaded.universal.redirector",
            UserName = "Reloaded-Project",
            AssetFileName = "reloaded.universal.monitor.zip",
            UseReleaseTag = false
        };

        var clonedDependency = _testEnvironmoent.TestModConfigBTuple.DeepClone();
        clonedDependency.Config.ReleaseMetadataFileName = "Reloaded.Universal.Redirector.ReleaseMetadata.json";
        Singleton<GitHubReleasesUpdateResolverFactory>.Instance.SetConfiguration(clonedDependency, config);
        var clonedOriginal = _testEnvironmoent.TestModConfigATuple.DeepClone();

        var gameBanana = new GitHubReleasesDependencyMetadataWriter();
        gameBanana.Update(clonedOriginal.Config, new[] { clonedDependency.Config });

        // Act
        using var outputDirectory = new TemporaryFolderAllocation();
        var resolver = new GitHubDependencyResolver();
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
        var resolver = new GitHubDependencyResolver();

        // Act
        var result = await resolver.ResolveAsync("this.package.does.not.exist");

        // Assert
        Assert.NotEmpty(result.NotFoundDependencies);
        Assert.Empty(result.FoundDependencies);
    }
}