namespace Reloaded.Mod.Installer;

/// <summary>
///     Settings for the installer.
/// </summary>
public struct Settings
{
    public string InstallLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Reloaded-II");
    public bool CreateShortcut { get; set; } = true;
    public bool StartReloaded { get; set; } = true;
    
    public Settings() { }
}