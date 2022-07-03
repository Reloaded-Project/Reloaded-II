namespace Reloaded.Mod.Loader.Utilities;

/// <summary>
/// Scans for known DRMs and prints appropriate warnings.
/// </summary>
public static class DRMHelper
{
    /// <summary>
    /// Known DRM Types.
    /// </summary>
    [Flags]
    public enum DrmType
    {
        None,
        SteamStub
    }

    public static DrmType CheckDrmAndNotify(BasicPeParser parser, Logger logger, out bool requiresDelayStart)
    {
        var drmType = DrmType.None;
        requiresDelayStart = false;

        if (SteamStubScanner.HasSteamStub(parser))
        {
            logger?.WriteLineAsync("Warning: Steam Stub (Embedded Steam DRM) found.\n" +
                                   "This means EXE is encrypted at launch. Support for bypassing this DRM is experimental.\n" +
                                   "If you find issues, remove the DRM using `Steamless` or try using ASI Loader `Edit Application -> Deploy ASI Loader`", logger.ColorWarning);

            requiresDelayStart = true;
            drmType |= DrmType.SteamStub;
        }

        return drmType;
    }
}