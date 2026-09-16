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

The Reloaded launcher sets the `RELOADEDIIMODS` environment variable to your
mods folder on first run; the CMake script deploys the DLL, `ModConfig.json`
and `ConfigSchema.json` there after each build, so the mod appears in the
launcher without manual copying. Without the variable the DLL is built into
`build/` and must be copied next to `ModConfig.json` by hand.

## Workflow

1. Edit `ConfigSchema.json` to declare your settings.
2. Read the values in C++ through `reloaded::config()` (see `main.cpp`).
3. Users change the settings in the launcher; values are saved to
   `<Reloaded>/User/Mods/<ModId>/Config.json` and read by your mod.

## Entry Point

The loader starts native mods by calling the first of these exports it finds:

- `ReloadedStartEx(const ReloadedStartInfo* info)` (recommended; provided by `RELOADED_MOD_CONFIG_IMPL`)
- `ReloadedStart`
- `InitializeASI`
- `Init`

`ReloadedStartEx` receives the mod and user-config directories, which is how the
helper finds the schema and the values file.

See the wiki page "Writing Native Mods" for the optional suspend/resume/unload exports.
