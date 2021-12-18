# High Level Architecture

![Reloaded II Architecture](./Diagrams/Images/Architecture-Diagram-Reloaded-II.png)

## Code Injection Steps
The typical bootstrapping mechanism for Reloaded II looks like the following:  
- Launcher starts the application as a suspended state.  
- Launcher uses DLL injection to inject the Loader `Bootstrapper`.  
- The `Bootstrapper` loads the .NET runtime and the Loader.  
- The Loader parses configurations and loads individual mods.  
- Process is resumed by Launcher.

Mods are loaded using a shared interface, implemented by mods. This works by forcing mods to load the same version of the interface that the main Loader was compiled against.

## Detecting Reloaded II

Reloaded II is not intended to bypass DRM or anti-cheating software. It makes no attempts to hide itself.  
Reloaded II can be easily detected through one of the following two methods.  

- Module List.  
If `Reloaded.Mod.Loader.dll` is found in the process' module list, there's a good chance that Reloaded is present inside the target process.

- Memory Mapped File.  
Alternatively, you can try opening a memory mapped file handle to `Reloaded-Mod-Loader-Server-PID-{a}` where `{a}` is the PID of the process you want to check. If the handle is valid, Reloaded is running inside the target process.

## Service: Local Server

Injected Reloaded instances inside the target process host a local UDP server which can easily be interacted with by connecting.  
The library `Reloaded.Mod.Loader.Server` can be used to interact with the server.  