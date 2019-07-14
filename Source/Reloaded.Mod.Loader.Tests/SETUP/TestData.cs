using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Misc;

namespace Reloaded.Mod.Loader.Tests.SETUP
{
    public class TestData : IDisposable
    {
        /// <summary>
        /// (Manually defined) List of non-existing dependencies.
        /// </summary>
        public List<string>     NonexistingDependencies { get; set; } = new List<string>();

        /// <summary>
        /// List of all mod configurations in the test data.
        /// </summary>
        public ModConfig[]      ModConfigurations { get; set; }

        /// <summary>
        /// List of all application configurations in the test data.
        /// </summary>
        public ApplicationConfig[]      AppConfigurations { get; set; }

        /// <summary>
        /// Backup of original config before being overwritten by tests.
        /// </summary>
        public LoaderConfig     OriginalConfig { get; set; }

        /// <summary>
        /// The mod loader configuration used for testing.
        /// </summary>
        public LoaderConfig     TestConfig { get; set; }

        /* Known configurations */
        public ApplicationConfig TestAppConfigA => AppConfigurations.First(x => x.AppId == "TestAppA");
        public ModConfig        TestModConfigA => ModConfigurations.First(x => x.ModId == "TestModA"); 
        public ModConfig        TestModConfigB => ModConfigurations.First(x => x.ModId == "TestModB");
        public ModConfig        TestModConfigC => ModConfigurations.First(x => x.ModId == "TestModC");

        public TestData()
        {
            // Backup config and override on filesystem with new.
            bool configExists = File.Exists(LoaderConfigReader.ConfigurationPath());
            if (configExists)
                OriginalConfig = LoaderConfigReader.ReadConfiguration();

            TestConfig = MakeTestConfig();
            LoaderConfigReader.WriteConfiguration(TestConfig);

            try
            {
                // Populate configurations.
                ModConfigurations = ModConfig.GetAllMods().Select(x => x.Object).ToArray();
                AppConfigurations = ApplicationConfig.GetAllApplications().Select(x => x.Object).ToArray();

                // Populate nonexisting dependencies.
                NonexistingDependencies.Add(TestModB.Program.NonexistingDependencyName);
                NonexistingDependencies.Add(TestModC.Program.NonexistingDependencyName);
            }
            catch (Exception e)
            {
                if (OriginalConfig != null)
                    LoaderConfigReader.WriteConfiguration(OriginalConfig);
                throw;
            }
        }

        public void Dispose()
        {
            if (OriginalConfig != null)
                LoaderConfigReader.WriteConfiguration(OriginalConfig);
            else
                File.Delete(LoaderConfigReader.ConfigurationPath());
        }

        /* Make LoaderConfig for Testing */
        public static LoaderConfig MakeTestConfig()
        {
            var config = new LoaderConfig();
            config.ApplicationConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.ApplicationConfigDirectory, "Apps");
            config.ModConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.ModConfigDirectory, "Mods");
            config.PluginConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.PluginConfigDirectory, "Plugins");
            config.EnabledPlugins = Constants.EmptyStringArray;
            return config;
        }

        private static string IfNotExistsMakeDefaultDirectoryAndReturnFullPath(string directoryPath, string defaultDirectory)
        {
            if (!Directory.Exists(directoryPath))
                return CreateDirectoryRelativeToCurrentAndReturnFullPath(defaultDirectory);

            return directoryPath;
        }

        private static string CreateDirectoryRelativeToCurrentAndReturnFullPath(string directoryPath)
        {
            string fullDirectoryPath = Path.GetFullPath(directoryPath);
            Directory.CreateDirectory(fullDirectoryPath);
            return fullDirectoryPath;
        }
    }
}
