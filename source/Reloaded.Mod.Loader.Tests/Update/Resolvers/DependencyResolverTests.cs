namespace Reloaded.Mod.Loader.Tests.Update.Resolvers;

public class DependencyResolverTests
{
    [Fact]
    public void Combine_ReturnsNoDuplicates()
    {
        const string id = "mod.duplicate.id";
        const string idNonExist = "mod.nonexist.id";

        var resultOld = new ModDependencyResolveResult();
        resultOld.FoundDependencies.Add(new DummyDownloadablePackage()
        {
            Id = id
        });

        resultOld.NotFoundDependencies.Add(idNonExist);

        var resultNew = new ModDependencyResolveResult();
        resultNew.FoundDependencies.Add(new DummyDownloadablePackage()
        {
            Id = id
        });

        resultNew.NotFoundDependencies.Add(idNonExist);

        // Act
        var resultCombined = ModDependencyResolveResult.Combine(new[] { resultOld, resultNew });

        // Assert
        Assert.Single(resultCombined.FoundDependencies);
        Assert.Single(resultCombined.NotFoundDependencies);
    }

    [Fact]
    public void Combine_ReturnsLatestVersion()
    {
        const string id = "mod.duplicate.id";
        var oldVersion = NuGetVersion.Parse("1.0.0");
        var newVersion = NuGetVersion.Parse("1.0.1");

        var resultOld = new ModDependencyResolveResult();
        resultOld.FoundDependencies.Add(new DummyDownloadablePackage()
        {
            Id = id,
            Version = oldVersion
        });
        
        var resultNew = new ModDependencyResolveResult();
        resultNew.FoundDependencies.Add(new DummyDownloadablePackage()
        {
            Id = id,
            Version = newVersion
        });

        // Act
        var resultCombined = ModDependencyResolveResult.Combine(new[] { resultOld, resultNew });

        // Assert
        Assert.Single(resultCombined.FoundDependencies);
        Assert.Equal(newVersion, resultCombined.FoundDependencies[0].Version);
    }
}