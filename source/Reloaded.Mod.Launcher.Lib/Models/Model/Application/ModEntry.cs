using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Application;

/// <summary>
/// Specialized version of <see cref="BooleanGenericTuple{TGeneric}"/> intended for storing bindable mod information.
/// </summary>
public class ModEntry : ObservableObject
{
    // ReSharper disable once UnusedMember.Global
    
    /// <summary/>
    public const string NameOfEnabled = nameof(Enabled);

    /// <summary>
    /// Whether the current mod is enabled.
    /// </summary>
    public bool? Enabled            { get; set; }

    /// <summary>
    /// Whether the state of the checkbox can be edited or not.
    /// </summary>
    public bool IsEditable          { get; set; }
    
    /// <summary>
    /// The mod to toggle the state for.
    /// </summary>
    public PathTuple<ModConfig> Tuple  { get; set; }

    /// <inheritdoc />
    public ModEntry(bool? enabled, PathTuple<ModConfig> tuple)
    {
        IsEditable = !tuple.Config.IsLibrary;
        Enabled = !IsEditable ? null : enabled;
        Tuple = tuple;
    }
}