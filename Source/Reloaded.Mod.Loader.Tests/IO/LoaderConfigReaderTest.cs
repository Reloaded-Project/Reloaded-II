using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class LoaderConfigReaderTest
    {
        private static LoaderConfigReader _configReader;

        /* Before Test */
        public LoaderConfigReaderTest()
        {
            _configReader = new LoaderConfigReader();
        }

        /* Simple Read/Write and Serialization Test */

        [Fact]
        public void ReadWriteConfig()
        {
            // Make new config first and backup old.
            var config = new LoaderConfig();
            bool configExists = File.Exists(_configReader.ConfigurationPath());
            LoaderConfig oldConfig = new LoaderConfig();

            if (configExists)
                oldConfig = _configReader.ReadConfiguration();

            // Add some random app support entries.
            config.ApplicationConfigDirectory = "Apps";
            config.ModConfigDirectory = "Mods";
            config.PluginConfigDirectory = "Plugins";
            config.InstallDirectory = Environment.CurrentDirectory;

            // Write and read back the config.
            _configReader.WriteConfiguration(config);
            var newConfig = _configReader.ReadConfiguration();

            // Restore old config and assert.
            if (configExists)
                _configReader.WriteConfiguration(oldConfig);
            else
                File.Delete(_configReader.ConfigurationPath());

            Assert.Equal(config, newConfig);
        }
    }
}
