namespace Reloaded.Mod.Loader.Update.Utilities;

/// <inheritdoc />
public class WebClientWithCompression : WebClient
{
    /// <inheritdoc />
    protected override WebRequest GetWebRequest(Uri address)
    {
        var request = base.GetWebRequest(address) as HttpWebRequest;
        request!.AutomaticDecompression = DecompressionMethods.All;
        return request;
    }
}