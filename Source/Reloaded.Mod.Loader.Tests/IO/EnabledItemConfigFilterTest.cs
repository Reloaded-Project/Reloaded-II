using System;
using System.Collections.Generic;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class EnabledItemConfigFilterTest : IDisposable
    {
        /// <summary>
        /// Number of random items to generate.
        /// </summary>
        private const int RandomItems = 100;

        /// <summary>
        /// Directory containing items to enable/disable.
        /// </summary>
        private static string SampleItemDirectory = "Items";

        /// <summary>
        /// Supports reading/writing of configurations.
        /// </summary>
        private static ConfigReader<ModConfig> _configReader = new ConfigReader<ModConfig>();

        /// <summary>
        /// Stores configurations used for testing.
        /// </summary>
        private List<PathGenericTuple<ModConfig>> _initialConfigurations;

        /// <summary>
        /// Random number generator used for determining whether an item is enabled.
        /// </summary>
        private Random _isEnabledRandomizer = new Random();

        private EnabledItemConfigFilter _enabledItemConfigFilter = new EnabledItemConfigFilter(Path.GetFullPath(SampleItemDirectory));

        /* Before unit test. */
        public EnabledItemConfigFilterTest()
        {
            Dispose();

            // Make mod configurations.
            for (int x = 0; x < RandomItems; x++)
            {
                // Random directory name.
                string tempDirectory = GetTemporaryDirectory();
                string configPath = Path.Combine(tempDirectory, ModConfig.ConfigFileName);

                // Make default mod config.
                _configReader.WriteConfiguration(configPath, new ModConfig());
            }

            _initialConfigurations = _configReader.ReadConfigurations(Path.GetFullPath(SampleItemDirectory), ModConfig.ConfigFileName);
        }

        /* After unit test. */
        public void Dispose()
        {
            if (Directory.Exists(SampleItemDirectory))
                Directory.Delete(SampleItemDirectory, true);
        }

        /// <summary>
        /// Checks the item enable and disable functionality.
        /// </summary>
        [Fact]
        public void EnableDisableItems()
        {
            // Get list of Enabled,Path,Item tuples. (All initially disabled)
            var items = GetItems();

            // Enable random items.
            foreach (var item in items)
                item.IsEnabled = RandomIsEnabled();

            // Store enabled items as relative paths.
            var enabledItems = _enabledItemConfigFilter.GetRelativePaths(items);

            // Get new items using enabled items collection, should have new enabled/disabled tags.
            var newItems = _enabledItemConfigFilter.GetItems(_initialConfigurations, enabledItems);

            // Order of `items` and `newItems` should be identical. If it is not this test will break here.
            string filteringItemCountMessage = "The number of items before and after filtering should be equal.";
            Assert.True(_initialConfigurations.Count == newItems.Count, filteringItemCountMessage);
            Assert.True(items.Count == newItems.Count, filteringItemCountMessage);

            for (int x = 0; x < items.Count; x++) // Verify Equality
                Assert.Equal(items[x].IsEnabled, newItems[x].IsEnabled);
        }
        
        /// <summary>
        /// Returns a list of all items and whether they are enabled/disabled.
        /// </summary>
        private List<EnabledPathGenericTuple<ModConfig>> GetItems()
        {
            return _enabledItemConfigFilter.GetItems(_initialConfigurations, new string[0]);
        }

        /// <summary>
        /// Randomly selects whether an item should be enabled or not.
        /// </summary>
        private bool RandomIsEnabled()
        {
            return Convert.ToBoolean(_isEnabledRandomizer.Next(2));
        }

        /// <summary>
        /// Creates a directory in temporary use inside the <see cref="SampleItemDirectory"/> folders.
        /// </summary>
        private string GetTemporaryDirectory()
        {
            string tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);

            string directoryPath = Path.Combine(SampleItemDirectory, Path.GetFileNameWithoutExtension(tempFileName));
            Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }
    }
}
