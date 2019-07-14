using System;
using System.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.SETUP;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class LoaderConfigReaderTest : IDisposable
    {
        /* Initialize/Dispose to protect existing config. */
        private TestData _testData = new TestData();

        public void Dispose()
        {
            _testData.Dispose();
        }

        /* Simple Read/Write and Serialization Test */
        [Fact]
        public void ReadWriteConfig()
        {
            // Make new config first and backup old.
            var config = TestData.MakeTestConfig();

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
