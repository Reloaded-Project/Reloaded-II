namespace Reloaded.Mod.Loader.Tests.IO;

public class ConfigReaderTests : IDisposable
{
    /* Name of the test configuration file and the default test configuration. */
    private const string ModConfigFileName = ModConfig.ConfigFileName;
    private const string AppConfigFileName = ApplicationConfig.ConfigFileName;

    /* Sample configs and loaders. */
    private ModConfig _testModConfig;
    private ApplicationConfig _testAppConfig;

    /* Before Tests */
    public ConfigReaderTests()
    {
        Dispose();

        _testModConfig = new ModConfig();
        _testAppConfig = new ApplicationConfig();
        _testAppConfig.SanitizeConfig();
        _testModConfig.SanitizeConfig();
    }

    /* After tests */
    public void Dispose()
    {
        /* May exist if tests fail. */
        File.Delete(ModConfigFileName);
        File.Delete(AppConfigFileName);
    }

    [Fact]
    public void WriteFile_ReadWithMaxDepth()
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

        try
        {
            string[] filePaths =
            {
                Path.Combine(directories[0], ModConfigFileName),
                Path.Combine(directories[1], AppConfigFileName)
            };

            /* Write to directories. */
            ConfigReader<ModConfig>.WriteConfiguration(filePaths[0], _testModConfig);
            ConfigReader<ApplicationConfig>.WriteConfiguration(filePaths[1], _testAppConfig);

            /* Find in directories. */
            var modPathConfigTuple = ConfigReader<ModConfig>.ReadConfigurations(currentDirectory, ModConfigFileName, default, 2)[0];
            var appPathConfigTuple = ConfigReader<ApplicationConfig>.ReadConfigurations(currentDirectory, AppConfigFileName, default, 2)[0];

            /* Validate Tuples. */
            Assert.Equal(Path.GetFullPath(filePaths[0]), Path.GetFullPath(modPathConfigTuple.Path));
            Assert.Equal(Path.GetFullPath(filePaths[1]), Path.GetFullPath(appPathConfigTuple.Path));
            Assert.Equal(_testModConfig, modPathConfigTuple.Config);
            Assert.Equal(_testAppConfig, appPathConfigTuple.Config);
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            /* Delete directories */
            foreach (string directory in directories)
                Directory.Delete(directory, true);
        }
    }

    /// <summary>
    /// Simple test that checks JSON Serialization/Deserialization.
    /// </summary>
    [Fact]
    public void WriteFile_ReadWithAnyDepth()
    {
        /* Write first. */
        ConfigReader<ModConfig>.WriteConfiguration(ModConfigFileName, _testModConfig);
        ConfigReader<ApplicationConfig>.WriteConfiguration(AppConfigFileName, _testAppConfig);

        /* Read back. */
        var modConfigCopy = ConfigReader<ModConfig>.ReadConfiguration(ModConfigFileName);
        var appConfigCopy = ConfigReader<ApplicationConfig>.ReadConfiguration(AppConfigFileName);

        /* Test for equality. */
        /* Need to cast or it will compare by reference/address (interfaces). */
        Assert.Equal(_testModConfig, modConfigCopy);
        Assert.Equal(_testAppConfig, appConfigCopy);
    }
}