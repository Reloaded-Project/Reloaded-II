namespace Reloaded.Mod.Loader.Tests.Update.Providers.GameBanana;

public class GameBananaApiTests
{
    [Fact]
    public async Task Search_ByGame_ReturnsAllResults()
    {
        var result = await GameBananaMod.GetByNameAllCategoriesAsync("", 7486, 0, 10);

        // Assert
        Assert.True(result.Count > 0);
        Assert.Contains(result, mod => mod.Name.Contains("Update Lib. Test"));
    }

    [Fact]
    public async Task Search_WithString_ReturnsTestLibrary()
    {
        var result = await GameBananaMod.GetByNameAllCategoriesAsync("Update Lib. Test", 7486, 0, 10);

        // Assert
        Assert.Single(result);
        Assert.True(result[0].Files.Count > 0);
    }

    [Fact]
    public async Task Search_WithSingleChar_DoesNotThrow()
    {
        var result = await GameBananaMod.GetByNameAllCategoriesAsync("u", 7486, 0, 10);

        // Assert
        Assert.True(result.Count > 0);
    }

    [Fact]
    public async Task Search_ReturnsModManagerIntegrations()
    {
        var result = await GameBananaMod.GetByNameAllCategoriesAsync("Update Lib. Test", 7486, 0, 10);

        // Assert
        var integration = result![0].ManagerIntegrations!.First();

        Assert.True(result![0].ManagerIntegrations!.Count > 0);
        Assert.True(integration!.Value[0]!.IsReloadedDownloadUrl()!.Value);
    }
}