<div align="center">
	<h1>Reloaded II</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
    Universal, C# based mod framework.
    <br/>
    Works with <i>anything</i> X86, X64.
</div>

## What is Reloaded II
**[Reloaded II]** is a Swiss army knife universal Game Modding framework.  

<div align="center">
	<img src="./Images/Header.png" width="550" align="center" />
	<br/><br/>
</div>

It is an ***extensible*** and ***modular*** framework that allows you to create your own mods for any game.  

## Mod Loader
Reloaded's Mod Loader is modular and extensible.  

- ✅ Supports any native 32-bit or 64-bit game.  
- ✅ Slim modular & extensible base.  
	- Loads mods, doesn't do anything unnecessary. Minimal design.  
    - Additional functionality (e.g. [File Redirection](https://github.com/Reloaded-Project/reloaded.universal.redirector)) provided by mods themselves.  

- ✅ Multiple load methods, injectable.  
	- Can be easily integrated into other mod loaders.  
	- Can be loaded with simple DLL injection.  

- ✅ Code mods using either .NET or native (C/C++/other) tools.  
- ✅ High performance.  
- ✅ External console & extensible file logging system.  
- ✅ Custom mod load order.  
	- You can assign priorities to different mods.  

- ✅ Up to date .NET Runtime.  
- ✅ Backwards compatibility for game mods.  
    - Older mods will not break due to mod loader changes.  

- ✅ Execute code before game does.  
	- You can execute code before the game runs a single line of code.  

- ✅ Mod hot-loading.  
	- You can load, unload, and reload mods at runtime.  
	- Or even remotely, server API available!  

- ✅ Debugging support in Visual Studio.  
	- Including Edit & Continue (note: Set `COMPLUS_FORCEENC = 1` environment variable).  

- ✅ .NET Mods are loaded in isolation.  
	- You can consume external libraries and/or NuGet Packages without worrying.  
	- Your mod using `Newtonsoft.Json 11.0.0` will not break because another mod is using `Newtonsoft.Json 13.0.0`.  
	- Where necessary can still break isolation on an `opt-in` basis (by setting a dependency).  

## Mod Framework & Launcher

- ✅ Automatic installer.  
	- Reloaded can be installed to any machine with a simple doubleclick.  
	- No dialogs, downloads all necessary runtimes.  
	- Very fast installer.  

- ✅ Familiar user interface.  
	- Includes tutorial to avoid confusion for new users.  

- ✅ Dependency system.  
	- Launcher automatically downloads missing mods.  
	- Loader automatically loads mods required by other mods first.  
	- Supports semantic versioning.  

- ✅ Supports multiple games at once.  
	- No need to have separate Reloaded copy for each game.  
	- Supports `Portable Mode`, in case you still want to.  

- ✅ Mod sets.  
	- Choose the mods you wanna load at next startup.  
	- Save/load list of mods to be loaded by the game.  
	- No need to move stuff in/out of mod folder.  

- ✅ Automatic updates.  
	- For everything: Launcher, Loader & Mods.  
	- Supports GitHub, NuGet, GameBanana and more in the future.  
	- Supports `Delta Updates` for smaller download sizes. Download only what has changed in the mod since last version!  
	- You can even host your own update servers.  

- ✅ Clean installation/uninstallation.  
	- Does not modify game directory at all!  
	- No registry keys etc. Remove Reloaded folder and it's gone.  

- ✅ Mod manifests/configuration.  
	- Supports no code, file only mods. (through other mods like [File Redirector](https://github.com/Reloaded-Project/reloaded.universal.redirector))  

- ✅ Integrated download options.  
	- Built-in mod downloader inside launcher.
	- 1 Click Downloads (available on supported websites e.g. GameBanana).  

- ✅ Movable directories.
	- You can move the `Mods` directory to any location you want.  

- ✅ Built-in GUI mod configuration.  
	- Mods can be configured directly from the launcher.  
	- Changes to configurations apply in real time (even when game is running!).  
	- Configurations are stored separately from mod files.  
	- Mods can support their own custom launchers/programs/code for configurations.  

- ✅ Theming support.  
	- The launcher can be extensively themed to user preference.  

- ✅ Multi-language launcher.  
	- The launcher can be translated to any language.  

## Platform Support

!!! todo

	This wiki needs troubleshooting information for non-Windows users.  
	If you have experience with setting up Reloaded on Wine, consider contributing to this wiki.  

Reloaded is natively a Windows application, however active effort is undertaken to ensure compatibility with Wine.  

| Operating System    | Description                          |
| ------------------- | ------------------------------------ |
| Windows             | ✅ Native                            |
| Linux               | ✅ Wine (+ Proton)                   |
| OSX                 | ✅ Wine (+ Proton)                   |
| Other               | ❓ Unknown.                          |

| Architecture   | Natively Supported    |
| -------------- | --------------------- |
| x86            | ✅                   |
| x86_64         | ✅                   |
| Windows on ARM | ❓ Unknown.           |
| ARM            | ❌                   |


## Contributions

Contributions to this project are **highly encouraged**.

Feel free to implement new features, make bug fixes or suggestions so long as they are accompanied by an issue with a clear description of the pull request.

Documentation is just as welcome as code changes!
