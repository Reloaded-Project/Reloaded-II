namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Checks if the bootstrapper is up to date.
/// </summary>
public class BootstrapperUpdateChecker
{
    private string _bootstrapperPath;

    /// <summary/>
    /// <param name="bootstrapperPath">Path to the bootstrapper DLL.</param>
    public BootstrapperUpdateChecker(string bootstrapperPath) => _bootstrapperPath = bootstrapperPath;

    /// <summary>
    /// Determines if the Dll needs updating.
    /// </summary>
    /// <returns>True if the bootstrapper needs an update, else false.</returns>
    public bool NeedsUpdate() => GetDllVersion() < EntryPointParameters.CurrentVersion;

    /// <summary>
    /// Gets the version of a bootstrapper DLL.
    /// </summary>
    /// <returns>A valid Bootstrapper version number (compare with <see cref="EntryPointParameters.CurrentVersion"/>).</returns>
    public int GetDllVersion()
    {
        var version = 0;
        try
        {
            version = Convert.ToInt32(FileVersionInfo.GetVersionInfo(_bootstrapperPath).ProductVersion);
        }
        catch (Exception e)
        {
            Errors.HandleException(e, $"[Bootstrapper update check] {Resources.ErrorAddApplicationGetVersionInfo.Get()}\n{e.Message}");
        }
        
        return version;
    }
}