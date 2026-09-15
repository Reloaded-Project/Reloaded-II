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

## Workflow

1. Build your DLL and place it next to `ModConfig.json` (path set in `ModNativeDll32/64`).
2. Edit `ConfigSchema.json` to declare your settings.
3. Read the values in C++ through `reloaded::config()` (see `main.cpp`).
4. Users change the settings in the launcher; values are saved to
   `<Reloaded>/User/Mods/<ModId>/Config.json` and read by your mod.

## Entry Point

The loader starts native mods by calling the first of these exports it finds:

- `ReloadedStartEx(const wchar_t* modDirectory, const wchar_t* userConfigDirectory)` (recommended; provided by `RELOADED_MOD_CONFIG_IMPL`)
- `ReloadedStart`
- `InitializeASI`
- `Init`

`ReloadedStartEx` receives the mod and user-config directories, which is how the
helper finds the schema and the values file.

See the wiki page "Writing Native Mods" for the optional suspend/resume/unload exports.
