using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.Model
{
    public class BooleanGenericTuple<TGeneric> : ObservableObject
    {
        public const string NameOfEnabled = nameof(Enabled);

        public bool Enabled { get; set; }
        public TGeneric Generic { get; set; }

        public BooleanGenericTuple(bool enabled, TGeneric generic)
        {
            Enabled = enabled;
            Generic = generic;
        }
    }
}
