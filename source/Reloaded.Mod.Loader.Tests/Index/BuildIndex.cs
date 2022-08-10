using System.IO;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.Update.Index;
using Reloaded.Mod.Loader.Update.Index.Structures.Config;
using Xunit;

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
        var api = new IndexApi($"{Assets.GameBananaIndexAssetsFolder}/");
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
}