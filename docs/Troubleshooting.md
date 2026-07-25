# Troubleshooting

## Steam DRM Prevents Debugging

You attached a debugger to your mod but breakpoints are never hit.

### Diagnosis

Open the Reloaded console before launching the game. If the executable uses Steam Stub DRM, Reloaded prints this warning:

```
Warning: Steam Stub (Embedded Steam DRM) found.
This means EXE is encrypted at launch. Support for bypassing this DRM is experimental.
If you find issues, remove the DRM using `Steamless` or try using ASI Loader `Edit Application -> Deploy ASI Loader`

Steam DRM prevents debugging. For workarounds, see: https://reloaded-project.github.io/Reloaded-II/Troubleshooting/#steam-drm-prevents-debugging
```

The presence of a `.bind` section in the executable triggers this message.

### Workaround

1. Download [Steamless](https://github.com/atom0s/Steamless).
2. Run Steamless and select the game executable to unpack it.
3. Replace the original game executable with the unpacked version.
4. Launch the game through Reloaded as normal.

The debugger should now hit breakpoints correctly.

---

## Debugging Bug: CS7069: Reference to type ‘...’ claims it is defined in ‘...’, but it could not be found.

**This is a bug of the Visual Studio Debugger**, [for more information, please look at and upvote this issue.](https://developercommunity.visualstudio.com/content/problem/864954/vs2019-debugger-unable-to-select-correct-net-core.html).

If you are a developer, you may encounter this issue if you run multiple mods which use different versions of the same DLL in their build output. 
At the moment, it seems Visual Studio cannot differentiate the assembly used by your mod's "AssemblyLoadContext"; and will always assume the first assembly with matching name is in use.

### Workaround
May not work if your mod has dependencies which use the same DLL but worth a try.

- Open `%appdata%/Reloaded-Mod-Loader-II/ReloadedII.json` in a text editor.
- Set `LoadModsInParallel` to `false`.
- Drag and drop your mod to the top of the mod list so it gets loaded first.