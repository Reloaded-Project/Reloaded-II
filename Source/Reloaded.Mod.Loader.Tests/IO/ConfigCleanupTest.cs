using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Tests.IO.Utilities;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    /// <summary>
    /// Validates whether the config cleanup helper functions work.
    /// </summary>
    public class ConfigCleanupTest : IDisposable
    {
        private const int RandomStringLength = 500;
        private const int RandomDirectoryLength = 5;

        private static Random _random = new Random();
        private TempLoaderConfigCreator _loaderConfigCreator;
        private LoaderConfig _tempLoaderConfig;

        /* Sample configs committed to disk for testing. */
        private ModConfig _realModConfig;
        private ApplicationConfig _realAppConfig;

        private ConfigLoader<ModConfig> _modConfigLoader;
        private ConfigLoader<ApplicationConfig> _appConfigLoader;

        /* Before and After Test. */
        public ConfigCleanupTest()
        {
            _loaderConfigCreator = new TempLoaderConfigCreator();
            _tempLoaderConfig = new LoaderConfigReader().ReadConfiguration();

            _modConfigLoader = new ConfigLoader<ModConfig>();
            _appConfigLoader = new ConfigLoader<ApplicationConfig>();

            // Make real sample configurations.
            _realModConfig = new ModConfig();
            _realAppConfig = new ApplicationConfig();

            string realModConfigFilePath = Path.Combine(_tempLoaderConfig.ModConfigDirectory, RandomString(RandomDirectoryLength), ModConfig.ConfigFileName);
            string realAppConfigFilePath = Path.Combine(_tempLoaderConfig.ApplicationConfigDirectory, RandomString(RandomDirectoryLength), ApplicationConfig.ConfigFileName);

            _modConfigLoader.WriteConfiguration(realModConfigFilePath, _realModConfig);
            _appConfigLoader.WriteConfiguration(realAppConfigFilePath, _realAppConfig);
        }

        public void Dispose()
        {
            _loaderConfigCreator.Dispose();
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
                ModDependencies = new[] { RandomString(RandomStringLength),
                                          RandomString(RandomStringLength),
                                          _realModConfig.ModId } // Default Mod ID
            };

            // Remove fake dependencies from config.
            testConfig.CleanupConfig(null);

            // Check that the only dependency left is the real one.
            Assert.True(testConfig.ModDependencies.Length == 1);
            Assert.True(testConfig.ModDependencies[0] == _realModConfig.ModId);
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
                EnabledMods = new[]
                {
                    RandomString(RandomStringLength),
                    RandomString(RandomStringLength),
                    RandomString(RandomStringLength),
                    _realModConfig.ModId
                }
            };

            // Remove fake mods from config.
            appConfig.CleanupConfig(null);

            // Check no dependencies exist.
            Assert.True(appConfig.EnabledMods.Length == 1);
            Assert.True(appConfig.EnabledMods[0] == _realModConfig.ModId);
        }

        /// <summary>
        /// Tests whether the app config can filter for nonexisting mods.
        /// </summary>
        [Fact]
        public void CleanupLoaderConfig()
        {
            // Make a new mod config.
            var appConfig = new LoaderConfig();
            appConfig.ModSupportMatrix.Add(RandomString(RandomStringLength), new string[0]); // Nonexisting Application without Mods
            appConfig.ModSupportMatrix.Add(RandomString(RandomStringLength), new string[] { RandomString(RandomStringLength),
                    RandomString(RandomStringLength), _realModConfig.ModId }); // Nonexisting Application with real Mod
            appConfig.ModSupportMatrix.Add(_realAppConfig.AppId, new string[] { RandomString(RandomStringLength),
                RandomString(RandomStringLength), _realModConfig.ModId }); // Existing Application with real Mod

            // Remove fake mod matrix entries.
            appConfig.CleanupConfig(null);

            // Check fake Applications were removed.
            Assert.True(appConfig.ModSupportMatrix.Keys.Count == 1);

            // Check that only one real mod in one real application remains.
            string firstKey = appConfig.ModSupportMatrix.Keys.ToArray()[0];
            string[] realAppModIds = appConfig.ModSupportMatrix[firstKey];

            Assert.True(realAppModIds.Length == 1); 
            Assert.True(realAppModIds[0] == _realModConfig.ModId);
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }


    }
}
