using Hashing = Reloaded.Mod.Loader.Community.Utility.Hashing;

namespace Reloaded.Mod.Loader.Tests.Community;

/// <summary>
/// This test checks for whether files can be correctly verified against a given GameItem.
/// </summary>
public class VerifyTest
{
    [Fact]
    public void Verify_WithoutHash_VerifiesCorrectly()
    {
        using var testFolder = new TemporaryFolderAllocation();
        const string expectedMessage = "Oh noes!";

        var gameItem   = new AppItem();

        var relativePath = Path.GetRandomFileName();
        var randomPath   = Path.Combine(testFolder.FolderPath, relativePath);
        File.Create(randomPath).Dispose();

        var warningItem = new WarningItem() { ErrorMessage = expectedMessage, };
        warningItem.Items.Add(new VerifyItem() { FilePath = relativePath });
        gameItem.Warnings.Add(warningItem);

        // Act
        var hasError = gameItem.TryGetError(testFolder.FolderPath, out var warnings);

        // Assert
        Assert.True(hasError);
        Assert.Single(warnings);
        Assert.Equal(expectedMessage, warnings[0].ErrorMessage);
    }

    [Fact]
    public void Verify_WithHash_VerifiesCorrectly()
    {
        using var testFolder = new TemporaryFolderAllocation();
        const string expectedMessage = "Oh noes!";

        var gameItem = new AppItem();

        var relativePath = Path.GetRandomFileName();
        var randomPath   = Path.Combine(testFolder.FolderPath, relativePath);
        
        // Write random file.
        var seed  = DateTime.UtcNow.Ticks % int.MaxValue;
        var bytes = new byte[1024];
        new Random((int)seed).NextBytes(bytes);
        File.WriteAllBytes(randomPath, bytes);
        
        // Add random file.
        var warningItem = new WarningItem() { ErrorMessage = expectedMessage, };
        warningItem.Items.Add(new VerifyItem() { FilePath = relativePath, Hash = Hashing.ToString(xxHash64.ComputeHash(bytes)) });
        gameItem.Warnings.Add(warningItem);
        
        // Act
        var hasError = gameItem.TryGetError(testFolder.FolderPath, out var warnings);

        // Assert
        Assert.True(hasError);
        Assert.Single(warnings);
        Assert.Equal(expectedMessage, warnings[0].ErrorMessage);
    }

    [Fact]
    public void Verify_WithoutMatchingFile_ReturnsEmpty()
    {
        using var testFolder = new TemporaryFolderAllocation();
        const string expectedMessage = "Oh noes!";

        var gameItem = new AppItem();

        var relativePath = Path.GetRandomFileName();
        var randomPath = Path.Combine(testFolder.FolderPath, relativePath);
        File.Create(randomPath + "random").Dispose();

        // Add random file.
        var warningItem = new WarningItem() { ErrorMessage = expectedMessage, };
        warningItem.Items.Add(new VerifyItem() { FilePath = relativePath });
        gameItem.Warnings.Add(warningItem);

        // Act
        var hasError = gameItem.TryGetError(testFolder.FolderPath, out var warnings);

        // Assert
        Assert.False(hasError);
        Assert.Empty(warnings);
    }
}