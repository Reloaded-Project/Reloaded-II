namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Allows for retrieval of Reloaded version.
/// </summary>
public static class Version
{
    private static NuGetVersion? _version;

    /// <summary>
    /// Gets the current release version of the Reloaded Launcher.
    /// </summary>
    public static NuGetVersion? GetReleaseVersion()
    {
        try
        {
            return GetReleaseVersion_Internal();
        }
        catch (Exception ex)
        {
            throw new Exception(Resources.ErrorUnableDetermineVersion.Get(), ex);
        }
    }
    
    private static NuGetVersion? GetReleaseVersion_Internal()
    {
        if (_version != null)
            return _version;

        try
        {
            if (File.Exists(Constants.VersionFilePath))
                return SetAndReturnVersion(NuGetVersion.Parse(File.ReadAllText(Constants.VersionFilePath)));
        }
        catch { /* Ignore */ }

        return SetAndReturnVersion(new NuGetVersion(Assembly.GetEntryAssembly()!.GetName().Version));
    }

    private static NuGetVersion? SetAndReturnVersion(NuGetVersion? version)
    {
        _version = version;
        return _version;
    }
}