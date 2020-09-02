using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Weaving;

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
        public ImageModPathTuple Tuple  { get; set; }

        public ModEntry(bool? enabled, ImageModPathTuple tuple)
        {
            IsEditable = !tuple.ModConfig.IsLibrary;
            Enabled = !IsEditable ? null : enabled;
            Tuple = tuple;
        }
    }
}
