using System.Diagnostics;
using Paths = Reloaded.Mod.Loader.IO.Paths;

namespace Reloaded.Mod.Loader.Tests.SETUP;

public class TestEnvironmoent : IDisposable
{
    /// <summary>
    /// Represents the ID of the executing test application.
    /// </summary>
    public const string     IdOfThisApp = "reloaded.mod.loader.tests";

    /// <summary>
    /// Represents the configuration path of this application's configuration.
    /// </summary>
    public string           ConfigurationPathOfThisApp { get; set; }

    /// <summary>
    /// (Manually defined) List of non-existing dependencies.
    /// </summary>
    public List<string>     NonexistingDependencies { get; set; } = new List<string>();

    /// <summary>
    /// List of all mod configurations in the test data.
    /// </summary>
    public ModConfig[]      ModConfigurations { get; set; }

    /// <summary>
    /// List of all mod configurations in the test data.
    /// </summary>
    public PathTuple<ModConfig>[] ModConfigurationTuples { get; set; }

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

    /// <summary>
    /// Service for reading mod configs.
    /// </summary>
    public ModConfigService ModConfigService { get; set; }

    /// <summary>
    /// Service for reading user configs.
    /// </summary>
    public ModUserConfigService UserConfigService { get; set; }

    /* Known configurations */
    public ApplicationConfig TestAppConfigA => AppConfigurations.First(x => x.AppId == "TestAppA");

    public ModConfig        TestModConfigA => ModConfigurations.First(x => x.ModId == "TestModA"); 
    public ModConfig        TestModConfigB => ModConfigurations.First(x => x.ModId == "TestModB");
    public ModConfig        TestModConfigC => ModConfigurations.First(x => x.ModId == "TestModC");
    public ModConfig        TestModConfigD => ModConfigurations.First(x => x.ModId == "TestModD"); // This config is a no DLL mod.
    public ModConfig        TestModConfigE => ModConfigurations.First(x => x.ModId == "TestModE"); // This config is a no DLL mod.

    public PathTuple<ModConfig> TestModConfigATuple => ModConfigurationTuples.First(x => x.Config.ModId == "TestModA");
    public PathTuple<ModConfig> TestModConfigBTuple => ModConfigurationTuples.First(x => x.Config.ModId == "TestModB");
    public PathTuple<ModConfig> TestModConfigCTuple => ModConfigurationTuples.First(x => x.Config.ModId == "TestModC");
    public PathTuple<ModConfig> TestModConfigDTuple => ModConfigurationTuples.First(x => x.Config.ModId == "TestModD"); // This config is a no DLL mod.
    public PathTuple<ModConfig> TestModConfigETuple => ModConfigurationTuples.First(x => x.Config.ModId == "TestModE"); // This config is a no DLL mod.

    public ApplicationConfig ThisApplication;

    public TestEnvironmoent()
    {
        // Backup config and override on filesystem with new.
        OriginalConfig = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath);
        TestConfig = MakeTestConfig();

        if (OriginalConfig.GetApplicationConfigDirectory() != TestConfig.GetApplicationConfigDirectory())
            IConfig<LoaderConfig>.ToPath(OriginalConfig, $"{Paths.LoaderConfigPath}.bak");
        
        IConfig<LoaderConfig>.ToPath(TestConfig, Paths.LoaderConfigPath);

        try
        {
            // Populate configurations.
            ModConfigurationTuples = ModConfig.GetAllMods().ToArray();
            ModConfigurations = ModConfigurationTuples.Select(x => x.Config).ToArray();
            AppConfigurations = ApplicationConfig.GetAllApplications().Select(x => x.Config).ToArray();

            ThisApplication = new ApplicationConfig(IdOfThisApp,
                "Reloaded Mod Loader Tests",
                Path.GetFullPath(Process.GetCurrentProcess().MainModule!.FileName!),
                new[] { TestModConfigA.ModId, TestModConfigB.ModId, TestModConfigD.ModId });

            ConfigurationPathOfThisApp = Path.Combine(TestConfig.GetApplicationConfigDirectory(), IdOfThisApp, ApplicationConfig.ConfigFileName);
            IConfig<ApplicationConfig>.ToPath(ThisApplication, ConfigurationPathOfThisApp);

            // Populate nonexisting dependencies.
            NonexistingDependencies.Add(TestModB.Program.NonexistingDependencyName);
            NonexistingDependencies.Add(TestModC.Program.NonexistingDependencyName);

            // Create user configs if necessary.
            ModConfigService = new ModConfigService(TestConfig);
            UserConfigService = new ModUserConfigService(TestConfig, ModConfigService);
        }
        catch (Exception)
        {
            IConfig<LoaderConfig>.ToPath(OriginalConfig, Paths.LoaderConfigPath);
            throw;
        }
    }

    public void Dispose()
    {
        IConfig<LoaderConfig>.ToPath(OriginalConfig, Paths.LoaderConfigPath);
    }

    /* Make LoaderConfig for Testing */
    public static LoaderConfig MakeTestConfig()
    {
        var config = new LoaderConfig();
        config.ApplicationConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.GetApplicationConfigDirectory(), "Apps");
        config.ModConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.GetModConfigDirectory(), "Mods");
        config.PluginConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.GetPluginConfigDirectory(), "Plugins");
        config.ModUserConfigDirectory = IfNotExistsMakeDefaultDirectoryAndReturnFullPath(config.GetModUserConfigDirectory(), "UserConfigs");
        config.EnabledPlugins = EmptyArray<string>.Instance;
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