using Paths = Reloaded.Mod.Loader.IO.Paths;

namespace Reloaded.Mod.Loader.Tests.IO;

public class LoaderConfigReaderTest : IDisposable
{
    /* Initialize/Dispose to protect existing config. */
    private TestEnvironmoent _testEnvironmoent = new TestEnvironmoent();

    public void Dispose()
    {
        _testEnvironmoent.Dispose();
    }

    /* Simple Read/Write and Serialization Test */
    [Fact]
    public void ReadWriteConfig()
    {
        // Make new config first and backup old.
        var config = TestEnvironmoent.MakeTestConfig();

        // Write and read back the config.
        IConfig<LoaderConfig>.ToPath(config, Paths.LoaderConfigPath);
        var newConfig = IConfig<LoaderConfig>.FromPath(Paths.LoaderConfigPath);

        // Restore old config and assert some members.
        Assert.Equal(config.LoaderPath32, newConfig.LoaderPath32);
        Assert.Equal(config.LoaderPath64, newConfig.LoaderPath64);
        Assert.Equal(config.LauncherPath, newConfig.LauncherPath);
        Assert.Equal(config.Bootstrapper32Path, newConfig.Bootstrapper32Path);
        Assert.Equal(config.Bootstrapper64Path, newConfig.Bootstrapper64Path);
        Assert.Equal(config.GetApplicationConfigDirectory(), newConfig.GetApplicationConfigDirectory());
        Assert.Equal(config.GetPluginConfigDirectory(), newConfig.GetPluginConfigDirectory());
        Assert.Equal(config.GetModConfigDirectory(), newConfig.GetModConfigDirectory());
        Assert.Equal(config.LanguageFile, newConfig.LanguageFile);
        Assert.Equal(config.ThemeFile, newConfig.ThemeFile);
    }

    /* Check for valid directory test. */
    [Fact]
    public void ValidDirectoryOnDeserialization()
    {
        // Make new config first and backup old.
        var config = new LoaderConfig();

        // Add some random app support entries.
        config.ApplicationConfigDirectory = "0Apps";
        config.ModConfigDirectory = "0Mods";
        config.PluginConfigDirectory = "0Plugins";

        // Write and read back the config.
        IConfig<LoaderConfig>.ToPath(config, Paths.LoaderConfigPath);
        var newConfig = IConfig<LoaderConfig>.FromPath(Paths.LoaderConfigPath);

        // Restore old config and assert.
        Assert.True(Directory.Exists(newConfig.GetApplicationConfigDirectory()));
        Assert.True(Directory.Exists(newConfig.GetModConfigDirectory()));
        Assert.True(Directory.Exists(newConfig.GetPluginConfigDirectory()));
    }
}