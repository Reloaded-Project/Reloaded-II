namespace Reloaded.Mod.Loader.Tests.Update.NuGet.Convert;

public class ConverterTests
{
    private static string TestArchiveFile = "HeroesControllerPostProcess.zip";
    private static string TestArchiveFileBad = "HeroesControllerPostProcessBad.zip"; // Has folder in root.

    [Fact]
    public async Task TryConvertBad()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(async () => await Converter.FromArchiveFileAsync(TestArchiveFileBad, Environment.CurrentDirectory));
    }

    [Fact]
    public async Task TryConvert()
    {
        var converted = await Converter.FromArchiveFileAsync(TestArchiveFile, Environment.CurrentDirectory);

        Assert.True(File.Exists(converted));
        Assert.True(IsZipValid(converted));
    }

    private static bool IsZipValid(string path)
    {
        using var zipFile = ZipFile.OpenRead(path);
        return zipFile.Entries.FirstOrDefault(x => x.Name.Contains("ModConfig.json")) != null;
    }
}