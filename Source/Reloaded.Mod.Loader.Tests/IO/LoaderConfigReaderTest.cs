using System;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.IO.Utilities;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class LoaderConfigReaderTest : IDisposable
    {
        private static LoaderConfigReader _configReader;
        private TempLoaderConfigCreator _loaderConfigCreator;

        /* Before Test */
        public LoaderConfigReaderTest()
        {
            _configReader = new LoaderConfigReader();
            _loaderConfigCreator = new TempLoaderConfigCreator();
        }

        /* After Test */
        public void Dispose()
        {
            _loaderConfigCreator.Dispose();
        }

        /* Simple Read/Write and Serialization Test */

        [Fact]
        public void ReadWriteConfig()
        {
            // Read back config first. (It will exist because of constructor)
            var config = new LoaderConfig();

            // Add some random app support entries.
            config.ModSupportMatrix.Add("reloaded.sample.app", new string[] { "sample.mod.a", "sample.mod.b" });
            config.ApplicationConfigDirectory = "Apps";
            config.ModConfigDirectory = "Mods";
            config.PluginConfigDirectory = "Plugins";
            config.InstallDirectory = Environment.CurrentDirectory;

            // Write and read back the config.
            _configReader.WriteConfiguration(config);
            var newConfig = _configReader.ReadConfiguration();

            Assert.Equal(config, newConfig);
        }
    }
}
