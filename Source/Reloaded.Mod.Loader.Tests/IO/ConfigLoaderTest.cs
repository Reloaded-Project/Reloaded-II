using System;
using System.IO;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.IO
{
    public class ConfigLoaderTest : IDisposable
    {
        /* Name of the test configuration file and the default test configuration. */
        private const string ModConfigFileName = ModConfig.ConfigFileName;
        private const string AppConfigFileName = ApplicationConfig.ConfigFileName;

        /* Sample configs and loaders. */
        private ModConfig _testModConfig;
        private ApplicationConfig _testAppConfig;

        private ConfigReader<ModConfig> _modConfigReader;
        private ConfigReader<ApplicationConfig> _appConfigReader;

        /* Before Tests */
        public ConfigLoaderTest()
        {
            Dispose();

            _testModConfig = new ModConfig();
            _testAppConfig = new ApplicationConfig();

            _modConfigReader = new ConfigReader<ModConfig>();
            _appConfigReader = new ConfigReader<ApplicationConfig>();
        }

        /* After tests */
        public void Dispose()
        {
            /* May exist if tests fail. */
            File.Delete(ModConfigFileName);
            File.Delete(AppConfigFileName);
        }

        [Fact]
        public void ReadWriteDirectory()
        {
            string MakeDirectory(string directory)
            {
                Directory.CreateDirectory(directory);
                return directory;
            }

            /* Get Directory and File Paths first. */
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] directories =
            {
                MakeDirectory("ModConfigDirectory"),
                MakeDirectory("AppConfigDirectory")
            };

            string[] filePaths =
            {
                Path.Combine(directories[0], ModConfigFileName),
                Path.Combine(directories[1], AppConfigFileName)
            };

            /* Write to directories. */
            _modConfigReader.WriteConfiguration(filePaths[0], _testModConfig);
            _appConfigReader.WriteConfiguration(filePaths[1], _testAppConfig);

            /* Find in directories. */
            var modPathConfigTuple = _modConfigReader.ReadConfigurations(currentDirectory, ModConfigFileName)[0];
            var appPathConfigTuple = _appConfigReader.ReadConfigurations(currentDirectory, AppConfigFileName)[0];

            /* Validate Tuples. */
            Assert.Equal(Path.GetFullPath(filePaths[0]), Path.GetFullPath(modPathConfigTuple.Path));
            Assert.Equal(Path.GetFullPath(filePaths[1]), Path.GetFullPath(appPathConfigTuple.Path));
            Assert.Equal(_testModConfig, modPathConfigTuple.Object);
            Assert.Equal(_testAppConfig, appPathConfigTuple.Object);

            /* Delete directories */
            foreach (string directory in directories)
                Directory.Delete(directory, true);
        }

        /// <summary>
        /// Simple test that checks JSON Serialization/Deserialization.
        /// </summary>
        [Fact]
        public void ReadWriteFile()
        {
            /* Write first. */
            _modConfigReader.WriteConfiguration(ModConfigFileName, _testModConfig);
            _appConfigReader.WriteConfiguration(AppConfigFileName, _testAppConfig);

            /* Read back. */
            var modConfigCopy = _modConfigReader.ReadConfiguration(ModConfigFileName);
            var appConfigCopy = _appConfigReader.ReadConfiguration(AppConfigFileName);

            /* Test for equality. */
            /* Need to cast or it will compare by reference/address (interfaces). */
            Assert.Equal(_testModConfig, modConfigCopy);
            Assert.Equal(_testAppConfig, appConfigCopy);
        }
    }
}
