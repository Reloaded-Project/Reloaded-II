namespace Reloaded.Mod.Loader.Tests.Update.Providers.NuGet;

/// <summary>
/// Tests for the NuGet package resolver.
/// </summary>
public class NuGetDependencyResolverTests
{
    [Fact]
    public async Task ResolveAsync_WithExistingPackage_ReturnsPackages()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider   = new NuGetDependencyResolver(new AggregateNugetRepository(new[] { repository }));

        // Act
        var result = await provider.ResolveAsync(NugetRepositoryTests.TestPackageName);

        // Assert
        Assert.NotEmpty(result.FoundDependencies);
        Assert.Empty(result.NotFoundDependencies);
    }

    [Fact]
    public async Task ResolveAsync_WithNoPackage_ReturnsMissing()
    {
        // Arrange
        var repository = NugetRepository.FromSourceUrl(NugetRepositoryTests.TestNugetFeed);
        var provider = new NuGetDependencyResolver(new AggregateNugetRepository(new[] { repository }));

        // Act
        var result = await provider.ResolveAsync("this.package.does.not.exist");

        // Assert
        Assert.NotEmpty(result.NotFoundDependencies);
        Assert.Empty(result.FoundDependencies);
    }
}