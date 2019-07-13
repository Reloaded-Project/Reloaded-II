using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Tests.SETUP.Utilities;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    /// <summary>
    /// Validates whether the config cleanup helper functions work.
    /// </summary>
    public class ConfigCleanupTest : IDisposable
    {
        /* Random string generator settings. */
        private const int RandomStringLength = 500;
        private const int RandomDirectoryLength = 5;

        /* Sample configs committed to disk for testing. */
        private ModConfig _realModConfig;
        private ApplicationConfig _realAppConfig;

        /* Classes setup for testing. */
        private LoaderConfig _tempLoaderConfig;
        private ConfigCleaner _configCleaner;

        /* Before and After Test. */
        public ConfigCleanupTest()
        {
            _tempLoaderConfig = LoaderConfig.GetTestConfig();

            var modConfigReader = new ConfigReader<ModConfig>();
            var appConfigReader = new ConfigReader<ApplicationConfig>();

            // Make real sample configurations.
            _realModConfig = new ModConfig();
            _realAppConfig = new ApplicationConfig();

            var realModConfigFilePath = Path.Combine(_tempLoaderConfig.ModConfigDirectory, RandomString.AlphaNumeric(RandomDirectoryLength), ModConfig.ConfigFileName);
            var realAppConfigFilePath = Path.Combine(_tempLoaderConfig.ApplicationConfigDirectory, RandomString.AlphaNumeric(RandomDirectoryLength), ApplicationConfig.ConfigFileName);

            modConfigReader.WriteConfiguration(realModConfigFilePath, _realModConfig);
            appConfigReader.WriteConfiguration(realAppConfigFilePath, _realAppConfig);
            _configCleaner = new ConfigCleaner(_tempLoaderConfig);
        }

        public void Dispose()
        {
            Directory.Delete(_tempLoaderConfig.ModConfigDirectory, true);
            Directory.Delete(_tempLoaderConfig.ApplicationConfigDirectory, true);
            Directory.Delete(_tempLoaderConfig.PluginConfigDirectory, true);
        }

        /// <summary>
        /// Tests whether the mod config can filter for nonexisting mods.
        /// </summary>
        [Fact]
        public void CleanupModConfig()
        {
            var testConfig = new ModConfig
            {
                ModIcon = RandomString.AlphaNumeric(RandomStringLength),
                ModDependencies = new[] { RandomString.AlphaNumeric(RandomStringLength),
                                          RandomString.AlphaNumeric(RandomStringLength),
                                          _realModConfig.ModId }, // Default Mod ID
                SupportedAppId = new[] { _realAppConfig.AppId,
                                         RandomString.AlphaNumeric(RandomStringLength),
                                         RandomString.AlphaNumeric(RandomStringLength) }
            };

            _configCleaner.CleanupModConfig(new PathGenericTuple<ModConfig>("", testConfig));

            Assert.True(testConfig.ModDependencies.Length == 1);
            Assert.True(testConfig.ModDependencies[0] == _realModConfig.ModId);
            Assert.True(String.IsNullOrEmpty(testConfig.ModIcon));

            Assert.True(testConfig.SupportedAppId.Length == 1);
            Assert.True(testConfig.SupportedAppId[0] == _realAppConfig.AppId);
        }

        /// <summary>
        /// Tests whether the app config can filter for nonexisting mods.
        /// </summary>
        [Fact]
        public void CleanupApplicationConfig()
        {
            var appConfig = new ApplicationConfig()
            {
                AppIcon = RandomString.AlphaNumeric(RandomStringLength),
                EnabledMods = new[]
                {
                    RandomString.AlphaNumeric(RandomStringLength),
                    RandomString.AlphaNumeric(RandomStringLength),
                    RandomString.AlphaNumeric(RandomStringLength),
                    _realModConfig.ModId
                }
            };

            _configCleaner.CleanupApplicationConfig(new PathGenericTuple<ApplicationConfig>("", appConfig));

            Assert.True(appConfig.EnabledMods.Length == 1);
            Assert.True(appConfig.EnabledMods[0] == _realModConfig.ModId);
            Assert.True(String.IsNullOrEmpty(appConfig.AppIcon));
        }

        /// <summary>
        /// Tests whether the app config can filter for nonexisting mods.
        /// </summary>
        [Fact]
        public void CleanupLoaderConfig()
        {
            var appConfig = new LoaderConfig();
            string fakeDirectory = "FAKEDIRECTORY";
            appConfig.ApplicationConfigDirectory = fakeDirectory;
            appConfig.ModConfigDirectory = fakeDirectory;
            appConfig.PluginConfigDirectory = fakeDirectory;

            _configCleaner.CleanupLoaderConfig(appConfig);

            Assert.True(Directory.Exists(appConfig.ApplicationConfigDirectory));
            Assert.True(Directory.Exists(appConfig.ModConfigDirectory));
            Assert.True(Directory.Exists(appConfig.PluginConfigDirectory));

            Assert.NotEqual(fakeDirectory, appConfig.ApplicationConfigDirectory);
            Assert.NotEqual(fakeDirectory, appConfig.ModConfigDirectory);
            Assert.NotEqual(fakeDirectory, appConfig.PluginConfigDirectory);
        }
    }
}
