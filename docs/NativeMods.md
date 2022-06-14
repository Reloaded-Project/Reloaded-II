# About Native Mods

Intended for communities who want to try out using Reloaded, transitioning to the mod loader or have a niche reason to use C/C++. Reloaded II has limited support for native C/C++ modifications compiled as DLLs. As standard, this is implemented through the use of DLL Exports.

Native mods lack access to components such as the mod loader API but can use some limited mod loader functionality, such as *Resume* and *Suspend* provided the right exports are available. 

## Mod Configuration

Just like any other mods, native mods with Reloaded require for `ModConfig.json`  to be present. This file must be present to allow the loader to know which DLL to load.

You can control which file the mod loader will load for x64 and x86 processes using the following config entries: 
```json
"ModNativeDll32": "LostWorldQuickBoot.dll",
"ModNativeDll64": "",
```
To generate the config file, create a new mod from within the launcher.

## Exports

**Entry Points:**

Reloaded tries to start mods by using the following entry points in order:

- ReloadedStart
- InitializeASI
- Init

If none of these entry points is found, the mod will not be loaded.
The exported methods should have no parameters and return `void`.

**Suspend, Resume, Unload:**

Reloaded II's *Resume*, *Suspend* and *Unload* functionalities are available for native mods. 
Virtually identical to their C# counterparts in the `IMod` interface, they require the following exports:

- ReloadedSuspend
- ReloadedResume
- ReloadedUnload
- ReloadedCanUnload
- ReloadedCanSuspend

`CanUnload` and `CanSuspend` are defined as `bool fn()` while `Suspend`, `Resume' and 'Unload` are defined as `void fn()`.

That said, if you are hooking/detouring functions **I would strongly advise against implementing these interfaces unless you know what you are doing.**

Specifically, you will need to use a good hooking/detouring library that fully respects stacked function hooks. It must allow for hook deactivation in a way that avoids touching both your C++ DLL and overwriting the original prologue of the hooked function. 

Here is an example of how such a hooking library may be implemented: [Reloaded.Hooks](https://github.com/Reloaded-Project/Reloaded.Hooks/issues/2).

## CoreRT/NativeAOT?
Yes you can; mad scientist. 
