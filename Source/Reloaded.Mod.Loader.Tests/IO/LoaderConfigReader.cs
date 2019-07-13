using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class LoaderConfigReader : IDisposable
    {
        /* Initialize/Dispose to protect existing config. */
        private LoaderConfig _backupConfig;

        public LoaderConfigReader()
        {
            bool configExists = File.Exists(Mod.Loader.IO.LoaderConfigReader.ConfigurationPath());
            if (configExists)
                _backupConfig = Mod.Loader.IO.LoaderConfigReader.ReadConfiguration();
        }

        public void Dispose()
        {
            // Delete if not exist prior, else restore.
            if (_backupConfig == null)
                File.Delete(Mod.Loader.IO.LoaderConfigReader.ConfigurationPath());
            else
                Mod.Loader.IO.LoaderConfigReader.WriteConfiguration(_backupConfig);
        }

        /* Simple Read/Write and Serialization Test */
        [Fact]
        public void ReadWriteConfig()
        {
            // Make new config first and backup old.
            var config = LoaderConfig.GetTestConfig();

            // Write and read back the config.
            Mod.Loader.IO.LoaderConfigReader.WriteConfiguration(config);
            var newConfig = Mod.Loader.IO.LoaderConfigReader.ReadConfiguration();

            // Restore old config and assert.
            Assert.Equal(config, newConfig);
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
            Mod.Loader.IO.LoaderConfigReader.WriteConfiguration(config);
            var newConfig = Mod.Loader.IO.LoaderConfigReader.ReadConfiguration();

            // Restore old config and assert.
            Assert.True(Directory.Exists(newConfig.ApplicationConfigDirectory));
            Assert.True(Directory.Exists(newConfig.ModConfigDirectory));
            Assert.True(Directory.Exists(newConfig.PluginConfigDirectory));
        }
    }
}
