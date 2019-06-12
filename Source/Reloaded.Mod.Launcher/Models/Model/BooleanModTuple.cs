using Reloaded.Mod.Interfaces;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.Model
{
    public class BooleanModTuple : ObservableObject
    {
        public bool Enabled { get; set; }
        public IModConfig ModConfig { get; set; }

        public BooleanModTuple(bool enabled, IModConfig modConfig)
        {
            Enabled = enabled;
            ModConfig = modConfig;
        }

        public override string ToString()
        {
            return ModConfig.ModName;
        }
    }
}
