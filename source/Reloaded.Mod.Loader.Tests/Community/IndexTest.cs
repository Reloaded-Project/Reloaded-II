using IndexApi = Reloaded.Mod.Loader.Community.IndexApi;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Routes = Reloaded.Mod.Loader.Community.Routes;

namespace Reloaded.Mod.Loader.Tests.Community;

/// <summary>
/// Tests the index generation abilities of the community library.
/// </summary>
public class IndexTest
{
    [Theory]
    [InlineData(100)]
    public void Index_CanBuild(int numItems)
    {
        using var indexSourceDir      = new TemporaryFolderAllocation();
        using var indexDestinationDir = new TemporaryFolderAllocation();

        // Act
        var index = BuildIndex(numItems, indexSourceDir.FolderPath, indexDestinationDir.FolderPath);

        // Assert
        var indexJsonExpectedPath = Path.Combine(indexDestinationDir.FolderPath, Routes.Index);
        Assert.True(File.Exists(indexJsonExpectedPath));
        Assert.Equal(numItems, index.IdToApps.Count);
    }

    [Fact]
    public async Task Index_CanRead()
    {
        const int numItems = 1;
        using var indexSourceDir = new TemporaryFolderAllocation();
        using var indexDestinationDir = new TemporaryFolderAllocation();

        // Act
        var index = BuildIndex(numItems, indexSourceDir.FolderPath, indexDestinationDir.FolderPath);
        var indexJsonExpectedPath = Path.Combine(indexDestinationDir.FolderPath, Routes.Index);

        Assert.True(File.Exists(indexJsonExpectedPath));
        var indexApi = new IndexApi($"{indexDestinationDir.FolderPath}/");

        // Assert
        var indexCopy = await indexApi.GetIndexAsync();
        Assert.Single(indexCopy.IdToApps);
    }

    [Fact]
    public async Task Index_CanDownload_ById()
    {
        const int numItems = 1;
        using var indexSourceDir = new TemporaryFolderAllocation();
        using var indexDestinationDir = new TemporaryFolderAllocation();

        var index = BuildIndex(numItems, indexSourceDir.FolderPath, indexDestinationDir.FolderPath);
        var indexJsonExpectedPath = Path.Combine(indexDestinationDir.FolderPath, Routes.Index);

        Assert.True(File.Exists(indexJsonExpectedPath));
        var indexApi = new IndexApi($"{indexDestinationDir.FolderPath}/");
        var indexCopy = await indexApi.GetIndexAsync();

        // Act
        var appId = indexCopy.IdToApps.First().Key;
        var item = indexCopy.FindApplication("", appId, out _);
        var application = await indexApi.GetApplicationAsync(item[0]);

        // Assert
        Assert.NotNull(application);
        Assert.Equal(appId, application.AppId);
        Assert.True(!string.IsNullOrEmpty(application.AppId));
        Assert.True(!string.IsNullOrEmpty(application.Hash));
    }

    [Fact]
    public async Task Index_CanDownload_ByHash()
    {
        const int numItems = 1;
        using var indexSourceDir = new TemporaryFolderAllocation();
        using var indexDestinationDir = new TemporaryFolderAllocation();

        var index = BuildIndex(numItems, indexSourceDir.FolderPath, indexDestinationDir.FolderPath);
        var indexJsonExpectedPath = Path.Combine(indexDestinationDir.FolderPath, Routes.Index);

        Assert.True(File.Exists(indexJsonExpectedPath));
        var indexApi = new IndexApi($"{indexDestinationDir.FolderPath}/");
        var indexCopy = await indexApi.GetIndexAsync();

        // Act
        var hash = indexCopy.HashToAppDictionary.First().Key;
        var item = indexCopy.FindApplication(hash, "", out _);
        var application = await indexApi.GetApplicationAsync(item[0]);

        // Assert
        Assert.NotNull(application);
        Assert.Equal(hash, application.Hash);
        Assert.True(!string.IsNullOrEmpty(application.AppId));
        Assert.True(!string.IsNullOrEmpty(application.Hash));
    }

    private static Mod.Loader.Community.Config.Index BuildIndex(int numItems, string sourceDir, string targetDir)
    {
        // Arrange: Write Applications
        var entries = GetGameEntryFaker().Generate(numItems);
        foreach (var entry in entries)
        {
            var relativePath = Routes.GetApplicationPath($"{entry.AppName}/{entry.AppId}{Routes.FileExtension}");
            var filePath = Path.Combine(sourceDir, relativePath);
            var json = JsonSerializer.Serialize(entry);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, json);
        }

        // Act
        return Mod.Loader.Community.Config.Index.Build(sourceDir, targetDir);
    }

    private static Faker<AppItem> GetGameEntryFaker()
    {
        return new Faker<AppItem>()
                .RuleFor(x => x.AppName, x => x.Internet.UserName())
                .RuleFor(x => x.AppId, x => x.System.FileName())
                .RuleFor(x => x.BadStatusDescription, x => x.Random.Words(10))
                .RuleFor(x => x.Hash, x => x.Random.Hash(16))
                .RuleFor(x => x.GameBananaId, x => x.Random.Number(0, 10000))
                .RuleFor(x => x.AppStatus, x => x.PickRandom<Status>());
    }
}