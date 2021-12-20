using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.Update.Exceptions;
using Reloaded.Mod.Loader.Update.Packaging.Converters.NuGet;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.NuGet.Convert;

public class ConverterTests
{
    private static string TestArchiveFile = "HeroesControllerPostProcess.zip";
    private static string TestArchiveFileBad = "HeroesControllerPostProcessBad.zip"; // Has folder in root.

    [Fact]
    public void TryConvertBad()
    {
        Assert.ThrowsAsync<BadArchiveException>(() => Converter.FromArchiveFileAsync(TestArchiveFileBad, Environment.CurrentDirectory));
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
        try
        {
            using var zipFile = ZipFile.OpenRead(path);
            var entries = zipFile.Entries;
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

}