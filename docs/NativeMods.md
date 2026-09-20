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

- [ReloadedStartEx][native-header] - `void fn(const ReloadedStartInfo* info)`
- ReloadedStart
- InitializeASI
- Init

If none of these entry points is found, the mod will not be loaded.

In the case of `ReloadedStartInfo`, it provides a wrapper around the API that's
usually provided to .NET mods (`IModLoader`).

After calling any API that returns strings, you will need to call `free_string`
afterwards.

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

## Languages

### C/C++

#### Setup

You need a C++17 compiler and CMake to build native mods:

- Visual Studio 2022 (or newer) with the *Desktop development with C++* workload,
  using the MSVC or Clang toolset.
- CMake 3.15 or newer, bundled with Visual Studio (or from [cmake.org](https://cmake.org)).

Start from the template (`dotnet new reloaded-native`) or copy the files from
the [native mod template][native-template].

Build the DLL for your game's architecture:

```text
cmake -B build -A x64        (64-bit game)
cmake -B build -A Win32      (32-bit game)
cmake --build build --config Release
```

Upon building, the mod will automatically be copied to the right location
and show up in Reloaded-II.

## Mod Configuration

### User Settings (Config Dialog)

The Reloaded-II launcher exposes a *Configure* dialog for native mods if the
`ConfigSchema.json` file exists next to `ModConfig.json`.

The declarative schema file supports all features supported by the .NET equivalent.

Example:

```json
{
  "Configurations": [
    {
      "FileName": "Config.json",
      "DisplayName": "Default Config",
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
          "Slider": {
            "Minimum": 0.0, "Maximum": 100.0,
            "SmallChange": 1.0, "LargeChange": 10.0,
            "TickFrequency": 10, "ShowTextField": true
          }
        },
        { "Name": "Brightness", "Type": "float", "DefaultValue": 1.5 },
        {
          "Name": "Quality",
          "Type": "enum",
          "DefaultValue": "High",
          "Values": [
            "Low",
            { "Name": "High", "DisplayName": "High Quality" }
          ]
        },
        {
          "Name": "CustomFile",
          "Type": "string",
          "FilePicker": { "Title": "Choose a File" }
        }
      ]
    }
  ]
}
```

- `Type` is `bool`, `int`, `float`, `double`, `string`, or an enum.
  Enums list their values inline under `Values`, or under a shared `Enums`
  array when the same enum is used by several properties.
- `DisplayName`, `Description`, `Category`, `Order` and `DefaultValue` mirror
  the attributes used by the C# mod template.
- `Slider`, `FilePicker` and `FolderPicker` mirror the `SliderControlParams`,
  `FilePickerParams` and `FolderPickerParams` attributes, all fields are
  optional.
- Each entry in `Configurations` becomes one page of the dialog, saved to its
  own file (`FileName`) inside the mod's user config folder
  (`User/Mods/<ModId>`). Values missing from the file fall back to
  `DefaultValue`.

The values are saved as a flat JSON file such as:

```json
{
  "EnableThing": false,
  "Volume": 10,
  "Brightness": 0.25,
  "Quality": "Low"
}
```

### Reading the Settings

#### C++

Using `ReloadedModConfig.h` from the [native mod template][native-template],
define `RELOADED_MOD_CONFIG_IMPL(your_start_function)` in
exactly one source file:

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

Missing values fall back to the schema defaults, then to the fallback
argument. `config.watch(callback)` reloads the settings when the user changes
them while the game is running.

[native-template]: https://github.com/Reloaded-Project/Reloaded-II/tree/master/source/Reloaded.Mod.Template/templates/native
[native-header]: https://github.com/Reloaded-Project/Reloaded-II/blob/master/source/Reloaded.Mod.Template/templates/native/ReloadedModConfig.h
