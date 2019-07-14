using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Tests.SETUP;
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

        /* Classes setup for testing. */
        private TestData _testData;
        private LoaderConfig _loaderConfig;
        private ConfigCleaner _configCleaner;

        /* Before and After Test. */
        public ConfigCleanupTest()
        {
            _testData = new TestData();
            _loaderConfig = _testData.TestConfig;
            _configCleaner = new ConfigCleaner(_loaderConfig);
        }

        public void Dispose()
        {
            _testData?.Dispose();
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
                                          _testData.TestModConfigA.ModId }, // Default Mod ID
                SupportedAppId = new[] { _testData.TestAppConfigA.AppId,
                                         RandomString.AlphaNumeric(RandomStringLength),
                                         RandomString.AlphaNumeric(RandomStringLength) }
            };

            _configCleaner.CleanupModConfig(new PathGenericTuple<ModConfig>("", testConfig));

            Assert.True(testConfig.ModDependencies.Length == 1);
            Assert.True(testConfig.ModDependencies[0] == _testData.TestModConfigA.ModId);
            Assert.True(String.IsNullOrEmpty(testConfig.ModIcon));

            Assert.True(testConfig.SupportedAppId.Length == 1);
            Assert.True(testConfig.SupportedAppId[0] == _testData.TestAppConfigA.AppId);
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
                    _testData.TestModConfigA.ModId
                }
            };

            _configCleaner.CleanupApplicationConfig(new PathGenericTuple<ApplicationConfig>("", appConfig));

            Assert.True(appConfig.EnabledMods.Length == 1);
            Assert.True(appConfig.EnabledMods[0] == _testData.TestModConfigA.ModId);
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
