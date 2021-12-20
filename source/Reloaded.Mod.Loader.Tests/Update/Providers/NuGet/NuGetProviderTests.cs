using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.Update.NuGet;
using Reloaded.Mod.Loader.Update.Providers.NuGet;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.Providers.NuGet;

public class NuGetProviderTests
{
    [Fact]
    public async Task SearchAsync_WithNoString_ReturnsResults()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider   = new NuGetPackageProvider(new AggregateNugetRepository(new []{ repository }));

        // Act
        var packages = await provider.SearchAsync("");

        // Assert
        Assert.True(packages.Count > 0);
    }

    [Fact]
    public async Task SearchAsync_WithString_ReturnsMatchingResults()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider = new NuGetPackageProvider(new AggregateNugetRepository(new[] { repository }));

        // Act
        var packages = await provider.SearchAsync("Hooks");

        // Assert
        Assert.True(packages.Count > 0);
        Assert.Contains(packages, package => package.Id.Equals("reloaded.sharedlib.hooks", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_CanDownloadReturnedPackage()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider   = new NuGetPackageProvider(new AggregateNugetRepository(new[] { repository }));

        // Act
        using var outputDirectory = new TemporaryFolderAllocation();
        var package = (await provider.SearchAsync("Hooks")).First();
        var downloadedPackagePath = await package.DownloadAsync(outputDirectory.FolderPath, null);

        // Assert
        Assert.True(Directory.Exists(downloadedPackagePath));
        Assert.True(File.Exists(Path.Combine(downloadedPackagePath, ModConfig.ConfigFileName)));
    }
}