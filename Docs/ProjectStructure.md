<div align="center">
	<h1>Reloaded II: Architecture</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>Who, what, when, where and why?</strong>
	<br/>
    What does each component do?
    <br/>
    Here's a short summary.
</div>

## Components

### Reloaded.Mod.Interfaces 
Project containing interfaces for mod loader components to be shared between the mod loader and other external components such as mods. This package contains no code, only interfaces and is the only requirement to get a Reloaded mod started.

Usually compiles down to tiny ~8KB DLL.

### Reloaded.Mod.Launcher
The WPF application responsible for the management of settings, mods, registered applications as well as loading and communicating with the mod loader.

At the time of writing it is running on .NET Framework 4.7.2 due to issues with the designer for .NET Core in Visual Studio. This will change in the future.

### Reloaded.Mod.Launcher.Kernel32AddressDumper
Simple utility program that is executed by `Reloaded.Mod.Launcher` during startup. 
It is used to extract the address of the `LoadLibraryW` function to allow for DLL Injection into x86 processes from an x64 one.

### Reloaded.Mod.Loader
The mod loader itself, it unironically loads mods.

For each mod, the loader provides isolation through a plugin system such that they can load each of their own dependencies without version clashes: For example one mod could load `Json.NET 10` and another `Json.NET 11`, despite both having the same DLL name.

The loader also supports a local server, which can be accessed by other clients such as the launcher. Said server provides support for e.g. Loading/Unloading/Suspending/Resuming mods and getting their state at runtime.

### Reloaded.Mod.Loader.Bootstrapper (C++)
Simple native bootstrapper DLL, which when injected into a process, loads the .NET Core Runtime and `Reloaded.Mod.Loader`.

This DLL can be copied (along with accompanying nethost library) to integrate the mod loader into other loaders. By default it loads asynchronously.

Due to Windows DLL Loader lock, it cannot unfortunately run synchronously to ensure mods execute before application code, however support is added for restarting the process via Reloaded's launcher, which will ensure mods load before game code.

This bootstrapper also ensures the loader is only loaded once into the process.

### Reloaded.Mod.Loader.Server
A library that contains the complete implementation for the mod loader server including all of the messages, responses and general driving code. 

It contains a fully featured client class for easy communication with the mod loader server.
Only the host class is contained in the actual mod loader itself.

### Reloaded.Mod.Loader.IO
Library which contains shared I/O code used to both load, save and clean up configuration files.

Used by unit tests, loader and the launcher.

### Reloaded.Mod.Shared
Tiny library that contains small shared pieces of code between the loader and the launcher.
Things that don't fit into any other library go here.