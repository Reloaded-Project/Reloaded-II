using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Windows.Input;
using McMaster.NETCore.Plugins;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command that allows you to configure an individual mod within Reloaded.
/// </summary>
public class ConfigureModCommand : WithCanExecuteChanged, ICommand
{
    private static Type[] _sharedTypes = { typeof(IConfigurator) };
    private readonly PathTuple<ModConfig>? _modTuple;

    /// <inheritdoc />
    public ConfigureModCommand(PathTuple<ModConfig>? modTuple)
    {
        _modTuple = modTuple;
    }
        
    /* ICommand */

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(object? parameter)
    {
        Execute_Internal();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
    }

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool CanExecute(object? parameter)
    {
        if (_modTuple != null)
        {
            var result = TryGetConfigurator(out _, out _); 
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            return result;
        }

        return false;
    }

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryGetConfigurator(out IConfigurator configurator, out PluginLoader loader)
    {
        var config = _modTuple!.Config;
        string dllPath = config.GetManagedDllPath(_modTuple.Path);
        configurator = null!;
        loader = null!;

        if (!File.Exists(dllPath))
            return false;

        loader = PluginLoader.CreateFromAssemblyFile(dllPath, true, _sharedTypes, config =>
        {
            config.DefaultContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!;
            config.IsLazyLoaded = true;
        });

        var assembly = loader.LoadDefaultAssembly();
        var types = assembly.GetTypes();
        var entryPoint = types.FirstOrDefault(t => typeof(IConfigurator).IsAssignableFrom(t) && !t.IsAbstract);

        if (entryPoint != null)
        {
            configurator = (IConfigurator)Activator.CreateInstance(entryPoint)!;
            configurator.SetModDirectory(Path.GetFullPath(Path.GetDirectoryName(_modTuple.Path)!));
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Execute_Internal()
    {
        // Important Note: We are keeping everything to the stack.
        // Want our best to ensure that no types leak out anywhere making unloadability hard.
        // Also, we must also keep loader used to load the configurator in stack, for obvious reasons.
        if (!TryGetConfigurator(out var configurator, out _)) 
            return;

        if (configurator.TryRunCustomConfiguration()) 
            return;

        Actions.ConfigureModDialog.Invoke(new ConfigureModDialogViewModel(configurator.GetConfigurations()));
    }
}