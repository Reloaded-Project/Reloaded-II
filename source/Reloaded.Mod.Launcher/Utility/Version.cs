using System.IO;
using System.Reflection;
using NuGet.Versioning;
using Reloaded.Mod.Launcher.Misc;

namespace Reloaded.Mod.Launcher.Utility;

public static class Version
{
    private static NuGetVersion _version;

    public static NuGetVersion GetReleaseVersion()
    {
        if (_version != null)
            return _version;

        if (File.Exists(Constants.VersionFilePath))
            return SetAndReturnVersion(NuGetVersion.Parse(File.ReadAllText(Constants.VersionFilePath)));

        return SetAndReturnVersion(new NuGetVersion(Assembly.GetEntryAssembly().GetName().Version));
    }

    private static NuGetVersion SetAndReturnVersion(NuGetVersion version)
    {
        _version = version;
        return _version;
    }
}