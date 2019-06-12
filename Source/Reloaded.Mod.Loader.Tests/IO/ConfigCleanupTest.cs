using System;
using System.IO;
using System.Linq;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    /// <summary>
    /// Validates whether the config cleanup helper functions work.
    /// </summary>
    public class ConfigCleanupTest
    {
        private const int RandomStringLength = 500;
        private const int RandomDirectoryLength = 5;

        private static Random _random = new Random();
        private LoaderConfig _tempLoaderConfig;

        /* Sample configs committed to disk for testing. */
        private ModConfig _realModConfig;
        private ApplicationConfig _realAppConfig;

        private string _realModConfigFilePath;
        private string _realAppConfigFilePath;

        private ConfigReader<ModConfig> _modConfigReader;
        private ConfigReader<ApplicationConfig> _appConfigReader;

        private ConfigCleaner _configCleaner;

        /* Before and After Test. */
        public ConfigCleanupTest()
        {
            _tempLoaderConfig = LoaderConfig.GetTestConfig();

            _modConfigReader = new ConfigReader<ModConfig>();
            _appConfigReader = new ConfigReader<ApplicationConfig>();

            // Make real sample configurations.
            _realModConfig = new ModConfig();
            _realAppConfig = new ApplicationConfig();

            _realModConfigFilePath = Path.Combine(_tempLoaderConfig.ModConfigDirectory, RandomString(RandomDirectoryLength), ModConfig.ConfigFileName);
            _realAppConfigFilePath = Path.Combine(_tempLoaderConfig.ApplicationConfigDirectory, RandomString(RandomDirectoryLength), ApplicationConfig.ConfigFileName);

            _modConfigReader.WriteConfiguration(_realModConfigFilePath, _realModConfig);
            _appConfigReader.WriteConfiguration(_realAppConfigFilePath, _realAppConfig);
            _configCleaner = new ConfigCleaner(_tempLoaderConfig);
        }

        /// <summary>
        /// Tests whether the mod config can filter for nonexisting mods.
        /// </summary>
        [Fact]
        public void CleanupModConfig()
        {
            // Make a new mod config.
            var testConfig = new ModConfig
            {
                ModIcon = RandomString(RandomStringLength),
                ModDependencies = new[] { RandomString(RandomStringLength),
                                          RandomString(RandomStringLength),
                                          _realModConfig.ModId }, // Default Mod ID
                SupportedAppId = new[] { _realAppConfig.AppId,
                                         RandomString(RandomStringLength),
                                         RandomString(RandomStringLength) }
            };

            // Remove fake dependencies from config.
            _configCleaner.CleanupModConfig(new PathGenericTuple<ModConfig>("", testConfig));

            // Check that the only dependency left is the real one.
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
            // Make a new mod config.
            var appConfig = new ApplicationConfig()
            {
                AppIcon = RandomString(RandomStringLength),
                EnabledMods = new[]
                {
                    RandomString(RandomStringLength),
                    RandomString(RandomStringLength),
                    RandomString(RandomStringLength),
                    _realModConfig.ModId
                }
            };

            // Remove fake mods from config.
            _configCleaner.CleanupApplicationConfig(new PathGenericTuple<ApplicationConfig>("", appConfig));

            // Check no dependencies exist.
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
            // Make a new mod config.
            var appConfig = new LoaderConfig();
            string fakeDirectory = "FAKEDIRECTORY";
            appConfig.ApplicationConfigDirectory = fakeDirectory;
            appConfig.ModConfigDirectory = fakeDirectory;
            appConfig.PluginConfigDirectory = fakeDirectory;

            // Remove fake mod matrix entries.
            _configCleaner.CleanupLoaderConfig(appConfig);

            // Check fake Applications were removed.
            Assert.True(Directory.Exists(appConfig.ApplicationConfigDirectory));
            Assert.True(Directory.Exists(appConfig.ModConfigDirectory));
            Assert.True(Directory.Exists(appConfig.PluginConfigDirectory));

            Assert.NotEqual(fakeDirectory, appConfig.ApplicationConfigDirectory);
            Assert.NotEqual(fakeDirectory, appConfig.ModConfigDirectory);
            Assert.NotEqual(fakeDirectory, appConfig.PluginConfigDirectory);
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
