namespace Reloaded.Mod.Loader.Tests.Update.Mocks;

[ExcludeFromCodeCoverage]
public class DummyDownloadablePackage : IDownloadablePackage
{
    public event PropertyChangedEventHandler PropertyChanged = (sender, args) => {};
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Authors { get; set; } = "";
    public Submitter? Submitter { get; set; } = new() { UserName = "" };
    public string Description { get; set; } = "";
    public string Source { get; set; } = "";
    public NuGetVersion Version { get; set; } = new NuGetVersion("1.0.0");
    public long? FileSize { get; } = 0;
    public string MarkdownReadme { get; } = null;
    public DownloadableImage[] Images { get; set; }
    public Uri ProjectUri { get; set; } = null;
    public long? LikeCount { get; } = null;
    public long? ViewCount { get; } = null;
    public long? DownloadCount { get; } = null;
    public DateTime? Published { get; set; } = null!;
    public string Changelog { get; } = null!;
    public string[] Tags { get; set; } = Array.Empty<string>();

    public Task<string> DownloadAsync(string packageFolder, IProgress<double> progress, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}