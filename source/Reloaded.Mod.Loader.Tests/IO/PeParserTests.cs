namespace Reloaded.Mod.Loader.Tests.IO;

public class PeParserTests
{
    [Fact]
    public void CanParsePEFile_32()
    {
        using var parser = new BasicPeParser("HelloWorld32.exe");

        Assert.True(parser.Is32BitHeader);
        Assert.Equal(5, parser.ImageSectionHeaders.Length);
        Assert.Equal(8, parser.ImportDescriptors.Length);
        Assert.Equal(16, parser.DataDirectories.Length);

        Assert.Equal(0x14C, parser.FileHeader.Machine);
        Assert.Equal(5, parser.FileHeader.NumberOfSections);

        Assert.Equal("KERNEL32.DLL", parser.GetImportDescriptorNames().Last(), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void CanParsePEFile_64()
    {
        using var parser = new BasicPeParser("HelloWorld64.exe");

        Assert.False(parser.Is32BitHeader);
        Assert.Equal(4, parser.ImageSectionHeaders.Length);
        Assert.Equal(9, parser.ImportDescriptors.Length);
        Assert.Equal(16, parser.DataDirectories.Length);

        Assert.Equal(0x8664, parser.FileHeader.Machine);
        Assert.Equal(4, parser.FileHeader.NumberOfSections);

        Assert.Equal("KERNEL32.DLL", parser.GetImportDescriptorNames().Last(), StringComparer.OrdinalIgnoreCase);
    }
}