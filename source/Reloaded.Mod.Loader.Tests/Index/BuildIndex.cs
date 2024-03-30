using IndexApi = Reloaded.Mod.Loader.Update.Index.IndexApi;
using IOEx = Sewer56.DeltaPatchGenerator.Lib.Utility.IOEx;
using Routes = Reloaded.Mod.Loader.Update.Index.Routes;

namespace Reloaded.Mod.Loader.Tests.Index;

public class IndexBuildTests : IndexTestCommon
{
    [Fact]
    public async Task BuildNuGetIndex()
    {
        const string outputFolder = "BuildNuGetIndexTest";

        // Arrange
        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(TestNuGetFeedUrl));

        // Act
        var index = await builder.BuildAsync(outputFolder);

        // Assert
        Assert.True(index.TryGetNuGetSourcePath(TestNuGetFeedUrl, out _));
        Assert.True(Directory.Exists(index.BaseUrl.LocalPath));
    }

    [Fact]
    public async Task BuildCombinedIndex()
    {
        const string outputFolder = "BuildCombinedIndexTest";
        const int heroesGameId = 6061;

        // Arrange
        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(TestNuGetFeedUrl));
        builder.Sources.Add(new IndexSourceEntry(heroesGameId));

        // Act
        var index = await builder.BuildAsync(outputFolder);

        // Assert
        Assert.True(index.TryGetNuGetSourcePath(TestNuGetFeedUrl, out _));
        Assert.True(index.TryGetGameBananaSourcePath(heroesGameId, out _));
        Assert.True(Directory.Exists(index.BaseUrl.LocalPath));
    }
    
    [Fact]
    public async Task BuildAllPackages()
    {
        const string outputFolder = "BuildAllPackagesTest";
        const int heroesGameId = 6061;

        // Arrange
        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(TestNuGetFeedUrl));
        builder.Sources.Add(new IndexSourceEntry(heroesGameId));

        // Act
        var index = await builder.BuildAsync(outputFolder);

        // Assert
        var api = new IndexApi(index.BaseUrl.ToString());
        var packages = await api.GetAllPackagesAsync();
        Assert.True(packages.Packages.Count > 0);
    }

    [Fact]
    public async Task BuildGbIndex()
    {
        const string outputFolder = "BuildGBIndexTest";
        const int heroesGameId = 6061;

        // Arrange
        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(heroesGameId));

        // Act
        var index = await builder.BuildAsync(outputFolder);

        // Assert
        Assert.True(index.TryGetGameBananaSourcePath(heroesGameId, out _));
        Assert.True(Directory.Exists(index.BaseUrl.LocalPath));
    }

    [Fact]
    public async Task UpdateExistingIndex()
    {
        const int heroesGameId = 6061;

        // Arrange
        using var tempFolder = new TemporaryFolderAllocation();
        IOEx.CopyDirectory(Assets.GameBananaIndexAssetsFolder, tempFolder.FolderPath);

        var api = new IndexApi($"{tempFolder.FolderPath}/");
        var index = await api.GetIndexAsync();

        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(TestNuGetFeedUrl));

        // Act
        index = await builder.UpdateAsync(index);

        // Assert
        Assert.True(index.TryGetNuGetSourcePath(TestNuGetFeedUrl, out _));
        Assert.True(index.TryGetGameBananaSourcePath(heroesGameId, out _));
        Assert.True(Directory.Exists(index.BaseUrl.LocalPath));
    }

    [Fact]
    public async Task RemoveItemFromIndex()
    {
        // Arrange
        using var tempFolder = new TemporaryFolderAllocation();
        IOEx.CopyDirectory(Assets.SampleIndexAssetsFolder, tempFolder.FolderPath);

        var api = new IndexApi($"{tempFolder.FolderPath}/");
        var index = await api.GetIndexAsync();

        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(TestNuGetFeedUrl));
        var gbIndex = index.Sources.First(x => x.Key.StartsWith(Routes.Source.IdentifierGameBanana)); // This will be removed.
        var nugetIndex = index.Sources.First(x => x.Key.StartsWith(Routes.Source.IdentifierNuGet));   // This will be preserved.

        // Act
        index = builder.RemoveNotInBuilder(index);

        // Assert
        Assert.Null(index.Sources.FirstOrDefault(x => x.Key.StartsWith(Routes.Source.IdentifierGameBanana)).Value);

        var oldNuGetFolder = Path.Combine(tempFolder.FolderPath, Path.GetDirectoryName(nugetIndex.Value));
        var oldGbFolder = Path.Combine(tempFolder.FolderPath, Path.GetDirectoryName(gbIndex.Value));
        Assert.True(Directory.Exists(oldNuGetFolder));
        Assert.False(Directory.Exists(oldGbFolder));
    }
}