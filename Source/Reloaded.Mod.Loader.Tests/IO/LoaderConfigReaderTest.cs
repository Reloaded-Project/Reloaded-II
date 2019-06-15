using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class LoaderConfigReaderTest
    {
        /* Simple Read/Write and Serialization Test */

        [Fact]
        public void ReadWriteConfig()
        {
            // Make new config first and backup old.
            var config = new LoaderConfig();
            bool configExists = File.Exists(LoaderConfigReader.ConfigurationPath());
            LoaderConfig oldConfig = new LoaderConfig();

            if (configExists)
                oldConfig = LoaderConfigReader.ReadConfiguration();

            // Add some random app support entries.
            config.ApplicationConfigDirectory = "Apps";
            config.ModConfigDirectory = "Mods";
            config.PluginConfigDirectory = "Plugins";
            config.InstallDirectory = Environment.CurrentDirectory;

            // Write and read back the config.
            LoaderConfigReader.WriteConfiguration(config);
            var newConfig = LoaderConfigReader.ReadConfiguration();

            // Restore old config and assert.
            if (configExists)
                LoaderConfigReader.WriteConfiguration(oldConfig);
            else
                File.Delete(LoaderConfigReader.ConfigurationPath());

            Assert.Equal(config, newConfig);
        }
    }
}
