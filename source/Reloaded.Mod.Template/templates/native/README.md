# Reloaded II Native Mod Template

A native (C/C++) mod for [Reloaded II](https://github.com/Reloaded-Project/Reloaded-II)
with launcher-side configuration. No C# DLL required.

## Files

| File | Purpose |
|---|---|
| `ModConfig.json` | Mod Info, tell the loader where to load the native DLL (`ModNativeDll32` / `ModNativeDll64`). |
| `ConfigSchema.json` | Declares the settings shown in the launcher's Configure dialog. |
| `ReloadedModConfig.h` | Header-only helper that reads the settings inside a mod. |
| `main.cpp` | Entry point. |
| `CMakeLists.txt` | Sample build script (MSVC or Clang; use `-A x64` or `-A Win32` to match the game). |

## Build

Requires a C++17 compiler (MSVC or Clang via the Visual Studio *Desktop
development with C++* workload) and CMake 3.15+, bundled with Visual Studio.

Build for your game's architecture:

```text
cmake -B build -A x64        (64-bit game, makes Reloaded.Native.Template.dll)
cmake -B build -A Win32      (32-bit game, makes Reloaded.Native.Template32.dll)
cmake --build build --config Release
```

Upon building, the mod will automatically be copied to the right location
and show up in Reloaded-II.

## Workflow

1. Edit `ConfigSchema.json` to declare your settings.
2. Read the values in C++ through `reloaded::config()` (see `main.cpp`).
3. Users change the settings in the launcher; values are saved to
   `<Reloaded>/User/Mods/<ModId>/Config.json` and read by your mod.
