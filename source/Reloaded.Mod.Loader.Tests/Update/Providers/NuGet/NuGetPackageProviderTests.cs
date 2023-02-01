namespace Reloaded.Mod.Loader.Tests.Update.Providers.NuGet;

public class NuGetPackageProviderTests
{
    [Fact]
    public async Task SearchAsync_WithNoString_ReturnsResults()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider   = new NuGetPackageProvider(repository);

        // Act
        var packages = await provider.SearchAsync("");

        // Assert
        Assert.True(packages.Any());
    }

    [Fact]
    public async Task SearchAsync_WithString_ReturnsMatchingResults()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider = new NuGetPackageProvider(repository);

        // Act
        var packages = await provider.SearchAsync("Hooks");

        // Assert
        Assert.True(packages.Any());
        Assert.Contains(packages, package => package.Id.Equals("reloaded.sharedlib.hooks", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_CanDownloadReturnedPackage()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider   = new NuGetPackageProvider(repository);

        // Act
        using var outputDirectory = new TemporaryFolderAllocation();
        var package = (await provider.SearchAsync("Hooks")).First();
        var downloadedPackagePath = await package.DownloadAsync(outputDirectory.FolderPath, null);

        // Assert
        Assert.True(Directory.Exists(downloadedPackagePath));
        Assert.True(File.Exists(Path.Combine(downloadedPackagePath, ModConfig.ConfigFileName)));
    }
}