using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.SETUP;
using Xunit;

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
        Assert.Equal(config.ApplicationConfigDirectory, newConfig.ApplicationConfigDirectory);
        Assert.Equal(config.PluginConfigDirectory, newConfig.PluginConfigDirectory);
        Assert.Equal(config.ModConfigDirectory, newConfig.ModConfigDirectory);
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
        Assert.True(Directory.Exists(newConfig.ApplicationConfigDirectory));
        Assert.True(Directory.Exists(newConfig.ModConfigDirectory));
        Assert.True(Directory.Exists(newConfig.PluginConfigDirectory));
    }
}