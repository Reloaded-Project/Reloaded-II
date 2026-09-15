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

## User Settings (Config Dialog)

Native mods can expose settings in the launcher's *Configure* dialog without any C# code, through a declarative schema file. Place a `ConfigSchema.json` file next to your `ModConfig.json` describing your settings, and the launcher builds the same configuration UI used by C# mods: checkboxes, numeric boxes, sliders, dropdowns, file and folder pickers, with categories, tooltips and a Reset button.

A minimal schema looks like this:

```json
{
  "Configurations": [
    {
      "FileName": "Config.json",
      "DisplayName": "Default Config",
      "Enums": [
        {
          "Name": "Quality",
          "Members": [ { "Name": "Low" }, { "Name": "High", "DisplayName": "High Quality" } ]
        }
      ],
      "Properties": [
        {
          "Name": "EnableThing",
          "Type": "bool",
          "DisplayName": "Enable Thing",
          "Description": "Turns the thing on or off.",
          "Category": "General",
          "Order": 0,
          "DefaultValue": true
        },
        {
          "Name": "Volume",
          "Type": "int",
          "DefaultValue": 75,
          "Slider": { "Minimum": 0.0, "Maximum": 100.0, "SmallChange": 1.0, "LargeChange": 10.0, "TickFrequency": 10, "ShowTextField": true }
        },
        { "Name": "Brightness", "Type": "float", "DefaultValue": 1.5 },
        { "Name": "Quality", "Type": "Quality", "DefaultValue": "High" },
        { "Name": "CustomFile", "Type": "string", "FilePicker": { "Title": "Choose a File" } }
      ]
    }
  ]
}
```

Notes:

- `Type` is one of `bool`, `int`, `float`, `double`, `string`, or the name of an entry in `Enums`.
- `DisplayName`, `Description`, `Category`, `Order` and `DefaultValue` mirror the attributes used by the C# mod template.
- `Slider`, `FilePicker` and `FolderPicker` mirror the `SliderControlParams`, `FilePickerParams` and `FolderPickerParams` attributes; all fields are optional.
- Each entry in `Configurations` becomes one page of the dialog, saved to its own file (`FileName`) inside the mod's user config folder (`User/Mods/<ModId>`). Values missing from the file fall back to `DefaultValue`.

The values are saved as a flat JSON file such as:

```json
{
  "EnableThing": false,
  "Volume": 10,
  "Brightness": 0.25,
  "Quality": "Low"
}
```

### Reading the Settings from C/C++

To read the settings inside your mod, copy `ReloadedModConfig.h` (from the [native mod template](https://github.com/Reloaded-Project/Reloaded-II/tree/master/source/Reloaded.Mod.Template/templates/native)) into your project and define `RELOADED_MOD_CONFIG_IMPL(your_start_function)` in exactly one source file. The macro exports `ReloadedStartEx`, which the loader calls with your mod's folders:

```cpp
#include "ReloadedModConfig.h"

static void my_start()
{
    auto& config = reloaded::config();
    bool enabled      = config.get_bool("EnableThing", true);
    long long volume  = config.get_int("Volume", 75);
    double brightness = config.get_float("Brightness", 1.5);
    std::wstring file = config.get_wstring("CustomFile", L"");

    static const char* quality[] = { "Low", "High" };
    int qualityIndex = config.get_enum("Quality", quality, 2, 1);
}

RELOADED_MOD_CONFIG_IMPL(my_start)
```

Missing values fall back to the schema defaults, then to the fallback argument. The header only needs the C++17 standard library (Windows APIs are used behind `_WIN32`, everything else uses `std::filesystem`), so it also works outside of Windows if you ever reuse it. `config.watch(callback)` spawns a thread that reloads the settings when the user changes them while the game is running. It hands you that thread, keep it and detach or join it, letting it go out of scope while it runs kills the process.

## Exports

**Entry Points:**

Reloaded tries to start mods by using the following entry points in order:

- ReloadedStartEx
- ReloadedStart
- InitializeASI
- Init

If none of these entry points is found, the mod will not be loaded.

`ReloadedStartEx` is defined as `void fn(const wchar_t* modDirectory, const wchar_t* userConfigDirectory)` and receives the mod's own folder (where `ConfigSchema.json` lives) and the folder where the launcher stores user settings. Use it (or the helper header above) if your mod reads its configuration. The other entry points should have no parameters and return `void`.

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
