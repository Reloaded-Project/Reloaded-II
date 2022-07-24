namespace Reloaded.Utils.Server.Configuration;

public class Config : Configurable<Config>
{
    /*
        User Properties:
            - Please put all of your configurable properties here.
    
        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs
    
        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */


    const string CategoryCommon = "Common Server Settings";

    [DefaultValue(false)]
    [Category(CategoryCommon)]
    [DisplayName("Log Actions")]
    [Description("If enabled, actions performed on the server are logged for debugging purposes.\n" +
                 "Otherwise, only errors are logged.")]
    public bool Log { get; set; } = false;

    public LiteNetLibConfig LiteNetLibConfig { get; set; } = new LiteNetLibConfig();
}