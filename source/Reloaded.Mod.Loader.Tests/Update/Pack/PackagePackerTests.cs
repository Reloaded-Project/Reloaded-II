using Reloaded.Mod.Loader.Update.Packs;
using Routes = Reloaded.Mod.Loader.Update.Packs.Routes;

namespace Reloaded.Mod.Loader.Tests.Update.Pack;

public class PackagePackerTests
{
    public string ImageFilePath = Path.Combine(Assets.PackAssetsFolder, "heroes-preview.jxl");
    public const string ModId = "test.mod";
    
    [Fact]
    public void Pack_Unpack_WithoutImages()
    {
        // Arrange
        var builder = BuildBaselinePack();

        // Act
        var result = builder.Build(out var pack);
        result.Position = 0;
        var reader = new ReloadedPackReader(result);
        var packCopy = reader.GetPack();

        // Assert
        Assert.Equal(pack, packCopy);
    }
    
    [Fact]
    public void Pack_Unpack_WithMainImage()
    {
        // Arrange
        var builder = BuildBaselinePack();
        var imageBytes = File.ReadAllBytes(ImageFilePath);    
        using var fs = new FileStream(ImageFilePath, FileMode.Open);
        builder.AddImage(fs, Path.GetExtension(ImageFilePath)!, "Sample Image");

        // Act
        var result = builder.Build(out var pack);
        result.Position = 0;
        var reader = new ReloadedPackReader(result);

        // Assert
        var newImage = reader.GetImage(reader.Pack.ImageFiles[0].Path);
        Assert.Equal(imageBytes, newImage);
    }
    
    [Fact]
    public void Pack_Unpack_WithMod()
    {
        // Arrange
        var builder = BuildBaselinePack();
        AddSampleMod(builder);

        // Act
        var result = builder.Build(out var pack);
        result.Position = 0;
        var reader = new ReloadedPackReader(result);
        var packCopy = reader.GetPack();

        // Assert
        Assert.Equal(pack, packCopy);
    }
    
    [Fact]
    public void Pack_Unpack_WithModImage()
    {
        // Arrange
        var builder = BuildBaselinePack();
        var modBuilder = AddSampleMod(builder);    
        var imageBytes = File.ReadAllBytes(ImageFilePath);
        using var fs = new FileStream(ImageFilePath, FileMode.Open);
        modBuilder.AddImage(fs, Path.GetExtension(ImageFilePath)!, "Sample Image");

        // Act
        var result = builder.Build(out var pack);
        result.Position = 0;
        var reader = new ReloadedPackReader(result);

        // Assert
        var newImage = reader.GetImage(reader.Pack.Items[0].ImageFiles[0].Path);
        Assert.Equal(imageBytes, newImage);
    }

    private ReloadedPackBuilder BuildBaselinePack()
    {
        var builder = new ReloadedPackBuilder();
        builder.SetName("Sample Package for Testing");
        builder.SetReadme("## Sample Readme");
        return builder;
    }

    private ReloadedPackItemBuilder AddSampleMod(ReloadedPackBuilder builder)
    {
        var modBuilder = builder.AddModItem(ModId);
        modBuilder.SetName("Sample Mod");
        modBuilder.SetReadme("Sample Readme");
        return modBuilder;
    }
}