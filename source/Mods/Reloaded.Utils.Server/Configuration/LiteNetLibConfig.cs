namespace Reloaded.Utils.Server.Configuration;

/// <summary>
/// All settings related to LiteNetLib.
/// </summary>
public class LiteNetLibConfig
{
    const string CategoryHost = "Host: LiteNetLib";

    [DefaultValue(true)]
    [Category(CategoryHost)]
    [DisplayName("Is Enabled?")]
    [Description("If true, the LiteNetLib UDP based API will be enabled.\n" +
        "This API is used in the Reloaded Launcher and recommended for use on local machine.")]
    public bool Enable { get; set; } = true;

    [DefaultValue("")]
    [Category(CategoryHost)]
    [DisplayName("Password")]
    [Description("The password required for remote hosts to join your server.")]
    public string Password { get; set; } = "";

    [DefaultValue((ushort)0)]
    [Category(CategoryHost)]
    [DisplayName("Port")]
    [Description("The port used.\n" +
                 "A value of 0 means your port will be chosen at random.")]
    public ushort Port { get; set; } = 0;

    [DefaultValue(false)]
    [Category(CategoryHost)]
    [DisplayName("Allow External Connections")]
    [Description("Allows for connections from outside this computer.\n" +
                 "Note: Will likely display Windows Firewall prompt.")]
    public bool AllowExternalConnections { get; set; } = false;

    public LiteNetLibConfig() { }
}