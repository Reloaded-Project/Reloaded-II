namespace Reloaded.Mod.Loader.Utilities.DRM;

public static class SteamStubScanner
{
    private const string SteamBindSection = ".bind";

    /// <summary>
    /// Returns true if Steam Stub DRM was found.
    /// </summary>
    public static unsafe bool HasSteamStub(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return HasSteamStub(fileStream, false);
    }

    /// <summary>
    /// Returns true if Steam Stub DRM was found.
    /// </summary>
    public static unsafe bool HasSteamStub(Stream stream, bool isMapped)
    {
        using var parser = new BasicPeParser(stream, isMapped);
        return HasSteamStub(parser);
    }

    /// <summary>
    /// Returns true if Steam Stub DRM was found.
    /// </summary>
    public static unsafe bool HasSteamStub(BasicPeParser parser)
    {
        return parser.ImageSectionHeaders.Any(x => x.Name.ToString() == SteamBindSection);
    }
}