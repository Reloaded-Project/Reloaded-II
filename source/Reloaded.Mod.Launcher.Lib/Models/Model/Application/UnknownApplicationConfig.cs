namespace Reloaded.Mod.Launcher.Lib.Models.Model.Application;

/// <summary>
/// Dummy application config for unknown supported apps in mods.
/// </summary>
/// <param name="appId">Name that uniquely identifies the Application.</param>
public class UnknownApplicationConfig(string appId) : IApplicationConfig
{
    /// <summary/>
    public string AppId { get; set; } = appId;

    /// <summary/>
    public string AppName { get; set; } = string.Empty;

    /// <summary/>
    public Dictionary<string, object> PluginData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <summary/>
    public string AppLocation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <summary/>
    public string AppArguments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <summary/>
    public string AppIcon { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <summary/>
    public string[] EnabledMods { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
