using Reloaded.Mod.Loader.IO.Interfaces;

namespace Reloaded.Mod.Launcher.Models.Model.IO
{
    public class LauncherConfig : IConfig
    {
        /// <summary>
        /// False on first launch, then true. For displaying welcome/help messages etc.
        /// </summary>
        public bool     FirstLaunch { get; set; }

        /// <summary>
        /// When launching Reloaded II again, autoselect last selected game.
        /// </summary>
        public string   LastApp { get; set; }

        /// <summary>
        /// Updates the loader automatically without user input.
        /// </summary>
        public bool     AutoAcceptUpdates { get; set; }
    }
}
