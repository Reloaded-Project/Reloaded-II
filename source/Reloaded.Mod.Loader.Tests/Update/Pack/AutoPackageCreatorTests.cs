using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.Tests.Update.Pack.Mocks;
using Reloaded.Mod.Loader.Update.Packs;
using Sewer56.Update.Resolvers.GameBanana;

namespace Reloaded.Mod.Loader.Tests.Update.Pack;

public class AutoPackageCreatorTests
{
    [Fact]
    public void CannotCreate_WithMissingUpdateInfo()
    {
        using var tempFolderAlloc = new TemporaryFolderAllocation();
        var conf = CreateTestConfig(tempFolderAlloc.FolderPath);
        
        Assert.False(AutoPackCreator.ValidateCanCreate(new []{ conf }, out var incompatible));
        Assert.Single(incompatible);
    }
    
    [Fact]
    public void CanCreate_WithUpdateInfo()
    {
        using var tempFolderAlloc = new TemporaryFolderAllocation();
        var conf = CreateTestConfig(tempFolderAlloc.FolderPath);
        AddGitHubUpdateResolver(conf);
        
        Assert.True(AutoPackCreator.ValidateCanCreate(new []{ conf }, out var incompatible));
        Assert.Empty(incompatible);
    }

    [Fact]
    public async Task AutoPackCreator_CanImportFromModSearch()
    {
        // Arrange
        using var tempMod = new TemporaryFolderAllocation();
        var modConf = CreateTestConfig(tempMod.FolderPath);
        AddGitHubUpdateResolver(modConf);

        var provider = GetGameBananaPackageProvider();
        
        // Act
        var pack = await AutoPackCreator.CreateAsync(new ModConfig[] { modConf.Config }, 
            new DummyImageConverter(), 
            new List<IDownloadablePackageProvider>() { provider});
        var built = pack.Build(out var package);
        
        // Assert
        File.WriteAllBytes("build.zip", built.ToArray());
        Assert.Single(package.Items);
        Assert.True(package.Items[0].ImageFiles.Count > 1);
    }
    
    [Fact]
    public async Task AutoPackCreator_CanDownloadMod()
    {
        // Arrange
        using var tempMod = new TemporaryFolderAllocation();
        var modConf = CreateTestConfig(tempMod.FolderPath);
        AddGitHubUpdateResolver(modConf);

        var provider = GetGameBananaPackageProvider();
        
        // Act
        var pack = await AutoPackCreator.CreateAsync(new ModConfig[] { modConf.Config }, 
            new DummyImageConverter(), new List<IDownloadablePackageProvider>() { provider});
        var built = pack.Build(out var package);

        var faulted = new List<ReloadedPack.TryDownloadResultForItem>();
        var success = await package.TryDownloadAsync(tempMod.FolderPath, faulted, new UpdaterData(new List<string>(), new CommonPackageResolverSettings()), null, default);
        
        // Assert
        Assert.True(success);
        Assert.NotEmpty(Directory.GetFiles(tempMod.FolderPath, "*.*", SearchOption.AllDirectories));
    }
    
    private PathTuple<ModConfig> CreateTestConfig(string folder)
    {
        return new PathTuple<ModConfig>(Path.Combine(folder, ModConfig.ConfigFileName), new ModConfig()
        {
            ModId = "sonicheroes.essentials.graphics",
            ModName = "Graphics Essentials" // so it finds on GameBanana
        });
    }
    
    private void AddGitHubUpdateResolver(PathTuple<ModConfig> modTuple)
    {
        var factory = new GitHubReleasesUpdateResolverFactory();
        factory.SetConfiguration(modTuple, new GitHubReleasesUpdateResolverFactory.GitHubConfig()
        {
            RepositoryName = "Heroes.Graphics.Essentials.ReloadedII",
            UserName = "Sewer56"
        });
    }

    private GameBananaPackageProvider GetGameBananaPackageProvider()
    {
        return new GameBananaPackageProvider(6061);
    }
}