using System.Collections.Generic;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Loader.IO.Config
{
    [Equals(DoNotAddEqualityOperators = true, DoNotAddGetHashCode = true)]
    public class ModUserConfig : ObservableObject, IConfig<ModUserConfig>, IModUserConfig
    {
        public Dictionary<string, object> PluginData { get; set; }

        public bool IsUniversalMod { get; set; }
    }
}
