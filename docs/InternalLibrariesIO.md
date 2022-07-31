# I/O Library

!!! info

    The [I/O library](https://www.nuget.org/packages/Reloaded.Mod.Loader.IO) is used for discovery, monitoring changes and parsing of Reloaded-II's configuration files.  

## Read/Write Loader Configuration

!!! info

    Shows how to read and write to the mod loader configuration.  

Read:
```csharp
var config = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath);
```

Write:
```csharp
IConfig<LoaderConfig>.ToPath(config, Paths.LoaderConfigPath);
```

The `IConfig` API can be used with all structures inside the `Reloaded.Mod.Loader.IO.Config` namespace, as such you can also use this API to read app or mod configurations.  

## Monitor Available Configurations

!!! info

    Shows how to use create services which keep track of all currently available configurations.  

    These services actively monitor the FileSystem and update their contents whenever a user deletes a mod, modifies a mod or creates a new mod.  

Mods:  
```csharp
// Read Loader Config
var config = IConfig<LoaderConfig>.FromPathOrDefault(Paths.LoaderConfigPath);

// Monitor Mods
var modConfigService = new ModConfigService(config);

// modConfigService.ItemsByFolder | A dictionary of all Folders -> Configurations
// modConfigService.ItemsByPath | A dictionary of all Config File Paths -> Configurations
// modConfigService.ItemsById | A dictionary of all ModIds -> Configurations
```

The following services are also available:  

| Service                  | Contents                                                                       |
|--------------------------|--------------------------------------------------------------------------------|
| ApplicationConfigService | All Application/Game Configurations.                                           |
| ModUserConfigService     | All User Specified Overrides for Mods, e.g. 'Allow Updating to Beta Versions'. |

## Batch Read Configurations

!!! info

    Shows how to read multiple configurations at once.  

The `ConfigReader<T>` class can be used for the batch reading of configurations.  

```csharp
// Read all mod configurations.
var configs = ConfigReader<ModConfig>.ReadConfigurations(loaderConfig.GetModConfigDirectory(), ModConfig.ConfigFileName, token, 2, 2);
```

## Useful Utility Methods

!!! info

    This section lists some static commonly used utility methods.  

| Method                                  | Purpose                                                                                                                                           |
|-----------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|
| ModConfig.GetAllMods                    | Reads all available mod configurations.                                                                                                           |
| ModConfig.SortMods                      | Sorts a list of mods taking into account their individual dependencies. i.e. Ensures that Mod A is loaded before Mod B if Mod B depends on Mod A. |
| ModConfig.GetDependencies               | Returns a list of all dependencies' mod configurations for a given mod.                                                                           |
| ModUserConfig.GetAllUserConfigs         | Retrieves all user configurations that override mod properties.                                                                                   |
| ModUserConfig.GetUserConfigPathForMod   | Retrieves the path of a user configuration for a given mod.                                                                                       |
| ModUserConfig.GetUserConfigFolderForMod | Retrieves the path where mod configuration files are stored.                                                                                      |
| ApplicationConfig.GetAllApplications    | Retrieves a list of all application configurations.                                                                                               |
| ApplicationConfig.GetAllMods            | Retrieves a list of all mods for a given application.                                                                                             |

## Other Utilities

A list of other utilities that may be helpful.  

| Class                        | Purpose                                                                                     |
|------------------------------|---------------------------------------------------------------------------------------------|
| Paths                        | Used to get the file paths of various Reloaded components, such as logs and configurations. |
| IOEx                         | Various utility methods for I/O operations.                                                 |
| FileSystemWatcherFactory     | Factory method(s) for creating functions that monitor FileSystem events.                    |
| NtQueryDirectoryFileSearcher | [Windows Only] Very, very fast directory and file searcher.                                 |
| BasicPeParser                | Basic fast parser for Windows PE files [EXE, DLL] with limited functionality.               |