namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IApplicationConfigV1
    {
        /// <summary>
        /// Name that uniquely identifies the Application.
        /// </summary>
        string AppId { get; set; }

        /// <summary>
        /// The visible name of the application. (As set by user and seen in launcher)
        /// </summary>
        string AppName { get; set; }

        /// <summary>
        /// The location of the main executable of the application.
        /// </summary>
        string AppLocation { get; set; }

        /// <summary>
        /// List of commandline arguments to run the application with if launching from the Reloaded Launcher.
        /// </summary>
        string AppArguments { get; set; }

        /// <summary>
        /// Location of the application icon image relative to the config file.
        /// </summary>
        string AppIcon { get; set; }

        /// <summary>
        /// Collection of enabled mods for this application by mod folder, sorted in load order.
        /// </summary>
        string[] EnabledMods { get; set; }
    }
}
