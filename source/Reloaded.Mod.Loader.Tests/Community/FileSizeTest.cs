using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Reloaded.Mod.Loader.Tests.Community;

/// <summary>
/// Tests the approximate file size of a large game database.
/// </summary>
public class FileSizeTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FileSizeTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(100)]   // Realistic number.
    //[InlineData(1000)]  // Super optimistic number.
    //[InlineData(50000)] // Approx. Steam game count.
    public void LargeIndex_Test(int numApplications)
    {
        var applications = GetGameEntryFaker().Generate(numApplications);
        var hashes = GetHashFaker().Generate(numApplications);
        var ids = GetIdFaker().Generate(numApplications);

        var index = new Mod.Loader.Community.Config.Index();
        for (int x = 0; x < applications.Count; x++)
        {
            index.IdToApps[ids[x]] = new List<IndexAppEntry>() { applications[x] };
            index.HashToAppDictionary[hashes[x]] = new List<IndexAppEntry>() { applications[x] };
        }

        var serialized = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(index));

        using var input        = new MemoryStream(serialized);
        using var output       = new MemoryStream();
        using var compressor   = new BrotliStream(output, CompressionLevel.Optimal, true);
        input.CopyTo(compressor);
        compressor.Dispose();

        _testOutputHelper.WriteLine($"[Num: {numApplications}] KBytes: {serialized.Length / 1024.0} | Brotli: {output.Length / 1024.0}");
    }

    private static Faker<IndexAppEntry> GetGameEntryFaker() =>
        new Faker<IndexAppEntry>()
            .RuleFor(x => x.AppName, x => x.Internet.UserName())
            .RuleFor(x => x.FilePath, x => $"{x.System.FileName()}/App.json");

    private static Faker<string> GetHashFaker() => new Faker<string>().CustomInstantiator((faker) => faker.Random.Hash(16));
    
    private static Faker<string> GetIdFaker() => new Faker<string>().CustomInstantiator((faker) => faker.System.FileName(".app"));
}