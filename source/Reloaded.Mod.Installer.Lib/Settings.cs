using System.Linq;
namespace Reloaded.Mod.Installer.Lib;

/// <summary>
///     Settings for the installer.
/// </summary>
public class Settings
{
    public string InstallLocation { get; set; } = Path.Combine(GetSafeInstallPath(), "Reloaded-II");
    public bool IsManuallyOverwrittenLocation { get; set; }
    public bool CreateShortcut { get; set; } = true;
    public bool HideNonErrorGuiMessages { get; set; } = false;
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
            if (args[x] == "--nogui") settings.HideNonErrorGuiMessages = true;
            if (args[x] == "--nocreateshortcut") settings.CreateShortcut = false;
            if (args[x] == "--nostartreloaded") settings.StartReloaded = false;
        }

        return settings;
    }

    private static string GetSafeInstallPath()
    {
        var installPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        bool hasNonAsciiChars = installPath.Any(c => c > 127);
        if (installPath.Contains("OneDrive") || hasNonAsciiChars)
        {
            var driveRoot = Path.GetPathRoot(Environment.SystemDirectory);
            if (driveRoot == null)
                // if for some reason we can't determine the root, fallback to Desktop
                return installPath;
            return driveRoot;
        }
        return installPath;
    }
}