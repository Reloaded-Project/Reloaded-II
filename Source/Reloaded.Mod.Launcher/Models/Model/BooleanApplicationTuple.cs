using Reloaded.Mod.Interfaces;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.Model
{
    public class BooleanApplicationTuple : ObservableObject
    {
        public bool Enabled { get; set; }
        public IApplicationConfig AppConfig { get; set; }

        public BooleanApplicationTuple(bool enabled, IApplicationConfig appConfig)
        {
            Enabled = enabled;
            AppConfig = appConfig;
        }

        public override string ToString()
        {
            return AppConfig.AppId;
        }
    }
}
