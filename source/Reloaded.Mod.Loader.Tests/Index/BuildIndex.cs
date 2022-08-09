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

        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(TestNuGetFeedUrl));
        await builder.BuildAsync(outputFolder);
    }

    [Fact]
    public async Task BuildGbIndex()
    {
        const string outputFolder = "BuildGBIndexTest";
        const int heroesGameId = 6061;

        var builder = new IndexBuilder();
        builder.Sources.Add(new IndexSourceEntry(heroesGameId));
        await builder.BuildAsync(outputFolder);
    }
}