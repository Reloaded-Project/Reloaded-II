using Reloaded.Mod.Loader.Update.Providers.Index;

namespace Reloaded.Mod.Loader.Tests.Update.Providers.Index;

public class IndexDependencyResolverTests
{
    [Fact]
    public async Task ResolveAsync_WithExistingPackage_ReturnsPackages()
    {
        // Act
        var resolver = new IndexDependencyResolver();
        var result = await resolver.ResolveAsync("reloaded.universal.redirector");

        // Assert
        Assert.NotEmpty(result.FoundDependencies);
        Assert.Empty(result.NotFoundDependencies);
    }

    [Fact]
    public async Task ResolveAsync_WithExistingPackage_CanDownloadPackage()
    {
        // Act
        using var outputDirectory = new TemporaryFolderAllocation();
        var resolver = new IndexDependencyResolver();
        var result = await resolver.ResolveAsync("reloaded.universal.redirector");
        var downloadedPackagePath = await result.FoundDependencies[0].DownloadAsync(outputDirectory.FolderPath, null);

        // Assert
        Assert.True(Directory.Exists(downloadedPackagePath));
        Assert.True(File.Exists(Path.Combine(downloadedPackagePath, ModConfig.ConfigFileName)));
    }

    [Fact]
    public async Task ResolveAsync_WithNoPackage_ReturnsMissing()
    {
        // Arrange
        var resolver = new IndexDependencyResolver();

        // Act
        var result = await resolver.ResolveAsync("this.package.does.not.exist");

        // Assert
        Assert.NotEmpty(result.NotFoundDependencies);
        Assert.Empty(result.FoundDependencies);
    }
}