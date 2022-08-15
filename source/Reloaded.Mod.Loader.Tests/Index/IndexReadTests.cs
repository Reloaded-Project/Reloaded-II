using IndexApi = Reloaded.Mod.Loader.Update.Index.IndexApi;

namespace Reloaded.Mod.Loader.Tests.Index;

public class IndexReadTests : IndexTestCommon
{
    [Fact]
    public async void ReadFromDisk()
    {
        // Arrange
        var indexPath = Assets.SampleIndexAssetsFolder;
        var indexApi = new IndexApi($"{indexPath}/");

        // Act
        var index = await indexApi.GetIndexAsync();
        
        // Assert
        Assert.NotNull(index);
        Assert.NotEmpty(index.Sources);

        // Assert has GB and NuGet
        const int heroesGameId = 6061;
        Assert.True(index.TryGetNuGetSourcePath(TestNuGetFeedUrl, out _), "NuGet Source not Found");
        Assert.True(index.TryGetGameBananaSourcePath(heroesGameId, out _), "GameBanana Source not Found");

        // Assert has releases
        var nugetPackageList = (await index.TryGetNuGetPackageList(TestNuGetFeedUrl));
        Assert.True(nugetPackageList.result, "NuGet Packages not Found");
        Assert.NotEmpty(nugetPackageList.list.Packages);

        var gbPackageList = (await index.TryGetGameBananaPackageList(heroesGameId));
        Assert.True(gbPackageList.result, "GameBanana Packages not Found");
        Assert.NotEmpty(gbPackageList.list.Packages);
    }
}