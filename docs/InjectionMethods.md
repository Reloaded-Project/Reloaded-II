# Introduction

There are many ways of loading Reloaded into a specific application or process.
Four are officially supported, and can be categorized as both **Synchronous** and **Asynchronous** with program startup.

**Synchronous** methods are recommended and allow for all of the mods to be initialized before the game or application starts to execute any code.

**Asynchronous** methods meanwhile load mods as the program is executing. That said some complex mods might not function as expected if loaded asynchronously.

## Manual Launch (Synchronous)

![Manual Launch](./Images/ManualLaunch.png)

**Summary:**

- Reloaded launches the application in a paused/suspended state.
- Reloaded is then loaded into the suspended application.
- The application is resumed.

## Auto-Inject/Inject (Asynchronous)
![Inject](./Images/Inject.png)

To access this menu, simply click the game instance under the "other instances" tab.

The "Inject" feature simply loads Reloaded into an existing process without having the need to restart it.

**Auto-inject** can be found inside the `Add an Application` menu, under `Advanced Tools & Options`. Auto-inject basically automatically injects Reloaded into programs as soon as they are launched.


## DLL Loader

Reloaded can be integrated into other ASI/DLL based mod loaders such as [Ultimate-ASI-Loader](https://github.com/ThirteenAG/Ultimate-ASI-Loader), by copying the **bootstrapper**. 

The bootstrapper is just a special DLL that loads .NET Core into an application and then boots Reloaded. **There are two bootstrappers**, one for 64-bit and one for 32-bit applications, and they can be found under the `Loader/X86/Bootstrapper` and `Loader/X64/Bootstrapper` directories respectively.

![Bootstrapper](./Images/Bootstrapper.png)

Installation will depend on the mod loader, but simply put if you copy the contents of this folder and make a mod loader mod, with `Reloaded.Mod.Loader.Bootstrapper.dll` as the target DLL (if possible), you can load Reloaded in other loaders.

### Synchronous Loading
By default, the bootstrapper DLL will load the mods **asynchronously**, meaning that they will be initialized as the game is normally running.

The bootstrapper however does have a feature to allow loading **"synchronously"**, by killing the game process and rebooting it. The way it works is that the bootstrapper will silently launch the launcher with a set of commandline arguments which instruct the launcher to re-launch the game, effectively performing the `Manual Launch` launch method.

To enable synchronous loading, simply make an empty file called  `ReloadedPortable.txt`  in the same directory as the bootstrapper, as seen in the example above.

**Notes:**

- The launcher will not add any additional commandline arguments, regardless of what you may have set for the application profile. Commandline arguments set in launcher apply to launching from launcher only.

- Reloaded can and will only be loaded once, the bootstrapper has a safety mechanism to ensure that.

### Integration Examples

#### Ultimate ASI Loader

With Ultimate ASI Loader, you can place the Bootstrapper in your scripts/plugins directory and rename the bootstrapper DLL with an `.asi` extension.

![Bootstrapper](./Images/DllLoaderExample2.png)

Booting via Ultimate ASI Loader is recommended in games where the embedded Steam DRM "Steam Stub" is present (game code is encrypted) as Reloaded II itself does not have a mechanism to handle this DRM. If you are using Ultimate ASI Loader to bypass Steam Stub, you should launch the game using its regular executable as opposed to using the launcher.

As of Reloaded II 1.1.0 and above users should not add `ReloadedPortable.txt`. Reloaded II provides the `InitializeASI` export, integrating with this loader.

---
**NOTE**

You can install Ultimate ASI Loader to a game from within Reloaded.</br>
Please see `Edit Application -> Advanced Tools & Options -> Deploy ASI Loader` inside the launcher.
---

#### SADX, SA2, Sonic R, Mania, SKC Mod Loader

![Bootstrapper](./Images/DllLoaderExample.png)

Mod.ini
```
Name=Reloaded Mod Loader II
Author=Sewer56
DLLFile=Reloaded.Mod.Loader.Bootstrapper.dll
```
