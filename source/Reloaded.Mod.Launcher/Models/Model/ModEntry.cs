using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Models.Model
{
    /// <summary>
    /// Specialized version of <see cref="BooleanGenericTuple{TGeneric}"/> intended for storing bindable mod information.
    /// </summary>
    public class ModEntry : ObservableObject
    {
        public const string NameOfEnabled = nameof(Enabled);

        public bool? Enabled            { get; set; }
        public bool IsEditable          { get; set; }
        public PathTuple<ModConfig> Tuple  { get; set; }

        public ModEntry(bool? enabled, PathTuple<ModConfig> tuple)
        {
            IsEditable = !tuple.Config.IsLibrary;
            Enabled = !IsEditable ? null : enabled;
            Tuple = tuple;
        }
    }
}
