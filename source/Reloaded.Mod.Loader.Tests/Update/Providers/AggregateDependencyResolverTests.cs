namespace Reloaded.Mod.Loader.Tests.Update.Providers;

public class AggregateDependencyResolverTests
{
    [Fact]
    public async Task ResolveAsync_ReturnsLatestDependency()
    {
        // Arrange
        const string PackageId = "super.cool.package";
        var oldVersion = NuGetVersion.Parse("1.0.0");
        var newVersion = NuGetVersion.Parse("1.0.1");

        var mockA = new Mock<IDependencyResolver>();
        mockA.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            FoundDependencies =
            { 
                new DummyDownloadablePackage()
                {
                    Id = PackageId,
                    Version = oldVersion
                }
            },
            NotFoundDependencies = {  }
        });

        var mockB = new Mock<IDependencyResolver>();
        mockB.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            FoundDependencies =
            {
                new DummyDownloadablePackage()
                {
                    Id = PackageId,
                    Version = newVersion
                }
            },
            NotFoundDependencies = {  }
        });

        var mockC = new Mock<IDependencyResolver>();
        mockC.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            FoundDependencies =
            {
                new DummyDownloadablePackage()
                {
                    Id = PackageId,
                    Version = oldVersion
                }
            },
            NotFoundDependencies = { }
        });

        // Act
        var aggregateResolver = new AggregateDependencyResolver(new[]
        {
            mockA.Object,
            mockB.Object,
            mockC.Object
        });

        var result = await aggregateResolver.ResolveAsync(PackageId);

        // Assert: Contains Package
        Assert.Contains(result.FoundDependencies, package => package.Id == PackageId && package.Version == newVersion);

        // Assert: No Duplicate
        Assert.DoesNotContain(result.FoundDependencies, package => package.Id == PackageId && package.Version == oldVersion);

        // Assert: All Found
        Assert.Empty(result.NotFoundDependencies);
    }

    [Fact]
    public async Task ResolveAsync_IncludesTransitiveDependencies()
    {
        // Arrange
        const string PackageId = "super.cool.package";
        const string PackageId2 = "super.cool.package2";
        var oldVersion = NuGetVersion.Parse("1.0.0");
        var newVersion = NuGetVersion.Parse("1.0.1");

        var mockA = new Mock<IDependencyResolver>();
        mockA.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            FoundDependencies =
            {
                new DummyDownloadablePackage()
                {
                    Id = PackageId,
                    Version = oldVersion
                },
                new DummyDownloadablePackage()
                {
                    Id = PackageId2,
                    Version = oldVersion
                },
            },
            NotFoundDependencies = { }
        });

        var mockB = new Mock<IDependencyResolver>();
        mockB.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            FoundDependencies =
            {
                new DummyDownloadablePackage()
                {
                    Id = PackageId,
                    Version = newVersion
                },
                new DummyDownloadablePackage()
                {
                    Id = PackageId2,
                    Version = newVersion
                },
            },
            NotFoundDependencies = { }
        });

        var mockC = new Mock<IDependencyResolver>();
        mockC.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            FoundDependencies =
            {
                new DummyDownloadablePackage()
                {
                    Id = PackageId,
                    Version = oldVersion
                },
                new DummyDownloadablePackage()
                {
                    Id = PackageId2,
                    Version = oldVersion
                },
            },
            NotFoundDependencies = { }
        });

        // Act
        var aggregateResolver = new AggregateDependencyResolver(new[]
        {
            mockA.Object,
            mockB.Object,
            mockC.Object
        });

        var result = await aggregateResolver.ResolveAsync(PackageId);

        // Assert: Contains Transitive Package Latest Ver
        Assert.Contains(result.FoundDependencies, package => package.Id == PackageId2 && package.Version == newVersion);

        // Assert: Does not Contain Transitive Package Old Ver
        Assert.DoesNotContain(result.FoundDependencies, package => package.Id == PackageId2 && package.Version == oldVersion);
    }

    [Fact]
    public async Task ResolveAsync_DoesNotDuplicateNotFound()
    {
        // Arrange
        const string PackageId = "super.cool.package";
        var oldVersion = NuGetVersion.Parse("1.0.0");
        var newVersion = NuGetVersion.Parse("1.0.1");

        var mockA = new Mock<IDependencyResolver>();
        mockA.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            NotFoundDependencies = { PackageId }
        });

        var mockB = new Mock<IDependencyResolver>();
        mockB.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            NotFoundDependencies = { PackageId }
        });

        var mockC = new Mock<IDependencyResolver>();
        mockC.Setup(x => x.ResolveAsync(PackageId, default, default)).ReturnsAsync(() => new ModDependencyResolveResult()
        {
            NotFoundDependencies = { PackageId }
        });

        // Act
        var aggregateResolver = new AggregateDependencyResolver(new[]
        {
            mockA.Object,
            mockB.Object,
            mockC.Object
        });

        var result = await aggregateResolver.ResolveAsync(PackageId);

        // Assert: Contains Package
        Assert.Single(result.NotFoundDependencies);
    }

}