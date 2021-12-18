

## Core Components

### Reloaded.Mod.Launcher + Reloaded.Mod.Launcher.Lib
The WPF application responsible for the management of settings, mods, registered applications as well as loading and communicating with the mod loader.  

- `Reloaded.Mod.Launcher` contains the view (WPF) portion.  
- `Reloaded.Mod.Launcher.Lib` contains the actual launcher code, including `ViewModels`.  

Powered by .NET 5.X at the time of last update.

### Reloaded.Mod.Loader
The mod loader itself, it unironically loads mods.  

For each mod, the loader provides isolation through a plugin system such that they can load each of their own dependencies without version clashes: For example one mod could load `Json.NET 10` and another `Json.NET 11`, despite both having the same DLL name.  

The loader also supports a local server, which can be accessed by other clients such as the launcher. Said server provides support for e.g. Loading/Unloading/Suspending/Resuming mods and getting their state at runtime.

### Reloaded.Mod.Loader.Bootstrapper (C++)
Simple native bootstrapper DLL, which when injected into a process, loads the .NET Runtime and `Reloaded.Mod.Loader`.  
This DLL can be copied to integrate the mod loader into other loaders seamlessly. 
Custom entry points for some common DLL Hijacking based loaders like [Ultimate ASI Loader](https://github.com/ThirteenAG/Ultimate-ASI-Loader) are also supported.  

By default it loads asynchronously.  
Due to Windows DLL Loader lock, it cannot unfortunately run synchronously to ensure mods execute before application code, however support is added for restarting the process via Reloaded's launcher, which will ensure mods load before application code.  

This bootstrapper also ensures the loader is only loaded once into the process.  

### Reloaded.Mod.Launcher.Kernel32AddressDumper
Simple utility program that is executed by `Reloaded.Mod.Launcher` during startup.  
It is used to extract the address of the `LoadLibraryW` function to allow for DLL Injection into x86 processes from an x64 one.

## Tools

### Reloaded.Mod.Installer
One click custom written installer for Reloaded.  
Installs all necessary dependencies and gets the mod loader up and running in under a minute.  

Written in .NET 4.7.2 which should be preinstalled in any up to date Windows version.  
Ships as `Setup.exe`.

### NuGetConverter
Simple program for generating NuGet packages which can be later uploaded to a repository and consumed within Reloaded.
Intended for usage in CI/CD scenarios, where updates need to be automatically generated without the use of a GUI.  

```
Reloaded-II NuGet Package Converter
Converts mod folders or archives into NuGet packages.
Usage: NuGetConverter.exe <Mod Folder or Archive Path> <Output Path>.
Example: NuGetConverter.exe Mod.zip Mod.nupkg
Example: NuGetConverter.exe reloaded.test.mod reloaded.test.mod.nupkg
Example: NuGetConverter.exe reloaded.test.mod ./packages/reloaded.test.mod.nupkg
```

### Reloaded.Publisher
Simple program that generates update files and releases without requiring the use of the full launcher.  
Intended for usage in CI/CD scenarios, where updates need to be automatically generated without the use of a GUI.  

## Libraries

### Reloaded.Mod.Interfaces 
Project containing interfaces for mod loader components to be shared between the mod loader and other external components such as mods. This package contains no code, only interfaces and is the only requirement to get a Reloaded mod started.  

Usually compiles down to tiny ~8KB DLL.

### Reloaded.Mod.Loader.IO
Contains the code for reading all and monitoring configuration files in Reloaded.  
Can be used as an external library outside of R-II.  

### Reloaded.Mod.Loader.Server
A library allowing you to send remote commands to an instance of Reloaded living inside a process.  
Contains the complete implementation for the mod loader server including all of the messages, responses and general driving code.  

It contains a fully featured client class for easy communication with the mod loader server.  
Only the host class is contained in the actual mod loader itself.

### Reloaded.Mod.Loader.Update
The part of Reloaded responsible for fetching and applying updates for the individual mod packages.  
Separated out from `Reloaded.Mod.Launcher.Lib` for potential reuse.  

### Reloaded.Mod.Loader.Update.Packaging
Subset of `Reloaded.Mod.Loader.Update` containing only the necessary code to create individual update packages.  
Separated out to keep tool binaries smaller.

### Reloaded.Mod.Installer.DependencyInstaller
Subset of `Reloaded.Mod.Loader.Update`.  
Contains the code to search for missing dependencies and install them.  
Separated out in otder to keep the `Reloaded.Mod.Installer` binary small.

### Reloaded.Mod.Shared
Contains items shared between the `Launcher`, `Loader` and `Kernel32AddressDumper`.  
Things that don't fit into any other library go here.