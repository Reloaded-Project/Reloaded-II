namespace Reloaded.Mod.Installer.Lib;

/// <summary>
///     Settings for the installer.
/// </summary>
public class Settings
{
    public string InstallLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Reloaded-II");
    public bool IsManuallyOverwrittenLocation { get; set; }
    public bool CreateShortcut { get; set; } = true;
    public bool StartReloaded { get; set; } = true;
    
    public Settings() { }
    
    public static Settings GetSettings(string[] args)
    {
        var settings = new Settings();
        for (int x = 0; x < args.Length - 1; x++)
        {
            if (args[x] == "--installdir")
            {
                settings.InstallLocation = args[x + 1];
                settings.IsManuallyOverwrittenLocation = true;
            }
            if (args[x] == "--nocreateshortcut") settings.CreateShortcut = false;
            if (args[x] == "--nostartreloaded") settings.StartReloaded = false;
        }

        return settings;
    }
}