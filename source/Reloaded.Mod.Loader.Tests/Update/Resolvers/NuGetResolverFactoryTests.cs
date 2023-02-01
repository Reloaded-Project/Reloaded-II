using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

namespace Reloaded.Mod.Loader.Tests.Update.Resolvers;

public class NuGetResolverFactoryTests : IDisposable
{
    private TestEnvironmoent _testEnvironmoent;

    public NuGetResolverFactoryTests() => _testEnvironmoent = new TestEnvironmoent();

    public void Dispose() => _testEnvironmoent.Dispose();

    [Fact]
    public void Migrate_MigratesNuSpecIntoMainConfig()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;
        var modDirectory = Path.GetDirectoryName(mod.Path);
        var nuspecFilePath = Path.Combine(modDirectory!, $"{IOEx.ForceValidFilePath(mod.Config.ModId)}.nuspec");
        File.Create(nuspecFilePath).Dispose();

        // Act
        var factory = new NuGetUpdateResolverFactory();
        factory.Migrate(mod, null);
        using var disposalHelper = new RemoveConfiguration<NuGetUpdateResolverFactory.NuGetConfig>(mod, factory);

        // Assert
        Assert.False(File.Exists(nuspecFilePath));
        Assert.True(factory.TryGetConfigurationOrDefault(mod, out var config));
    }

    [Fact]
    public void GetResolver_UsesMainConfigUrls_BeforeSecurityPolicyMigrationDate()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;
        
        // Act
        var resolverFactory  = new NuGetUpdateResolverFactory();
        NuGetUpdateResolverFactory.SetNowTime(NuGetUpdateResolverFactory.MigrationDate.AddMinutes(-1));
        var resolvers = (AggregatePackageResolver) resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>() { "Sample NuGet Feed" }, new CommonPackageResolverSettings()));

        // Assert
        Assert.Equal(1, resolvers.Count);
    }

    [Fact]
    public void GetResolver_RespectsModConfigUrls()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;

        // Act
        var resolverFactory = new NuGetUpdateResolverFactory();
        resolverFactory.SetConfiguration(mod, new NuGetUpdateResolverFactory.NuGetConfig()
        {
            DefaultRepositoryUrls = new ObservableCollection<StringWrapper>() { "Sample Repository" }
        });

        using var disposalHelper = new RemoveConfiguration<NuGetUpdateResolverFactory.NuGetConfig>(mod, resolverFactory);

        var resolver = (AggregatePackageResolver)resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>(), new CommonPackageResolverSettings()));

        // Assert
        Assert.Equal(1, resolver.Count);
        Assert.True(resolverFactory.TryGetConfigurationOrDefault(mod, out var config));
    }

    [Fact]
    public void GetResolver_DoesNotDuplicateFeeds()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;

        // Act
        var resolverFactory = new NuGetUpdateResolverFactory();
        resolverFactory.SetConfiguration(mod, new NuGetUpdateResolverFactory.NuGetConfig()
        {
            DefaultRepositoryUrls = new ObservableCollection<StringWrapper>() { "Sample Repository" }
        });

        using var disposalHelper = new RemoveConfiguration<NuGetUpdateResolverFactory.NuGetConfig>(mod, resolverFactory);

        var resolver = (AggregatePackageResolver)resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>() { "Sample Repository" }, new CommonPackageResolverSettings()));

        // Assert
        Assert.Equal(1, resolver.Count);
    }

    [Fact]
    public void GetResolver_ReturnsNullOnNoFeeds()
    {
        // Arrange
        var mod = _testEnvironmoent.TestModConfigATuple;

        // Act
        var resolverFactory = new NuGetUpdateResolverFactory();
        var resolver = resolverFactory.GetResolver(mod, null, new UpdaterData(new List<string>() {  }, new CommonPackageResolverSettings()));

        // Assert
        Assert.Null(resolver);
    }
}