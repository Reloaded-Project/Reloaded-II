namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Class that helps read Reloaded packages.
/// </summary>
public class ReloadedPackReader
{
    /// <summary>
    /// The configuration stored inside the pack.
    /// </summary>
    public ReloadedPack Pack { get; private set; } = null!;

    private ZipArchive _archive;
    
    /// <summary>
    /// Creates a reader with the purpose of reading the Reloaded pack package.
    /// </summary>
    /// <param name="stream">The stream containing the package (.r2pack) to be read.</param>
    public ReloadedPackReader(Stream stream)
    {
        _archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        var config = ExtractFile(Routes.Config);
        Pack = JsonSerializer.Deserialize<ReloadedPack>(config)!;
    }

    /// <summary>
    /// Gets the configuration behind the Reloaded pack.
    /// </summary>
    public ReloadedPack GetPack() => Pack;

    /// <summary>
    /// Retrieves an image from the Reloaded pack.
    /// </summary>
    /// <param name="fileName">Name of the image in the package.</param>
    /// <returns>Data for the image.</returns>
    public byte[] GetImage(string fileName) => ExtractFile(Routes.GetImagePath(fileName));

    /// <summary>
    /// Extracts a file with the given path from inside the archive.
    /// </summary>
    /// <param name="path">Path of the file to extract.</param>
    /// <returns>Data for the file, empty if file does not exist.</returns>
    public byte[] ExtractFile(string path)
    {
        var entry = _archive.GetEntry(path);
        if (entry == null)
            return Array.Empty<byte>();

        var outputData = GC.AllocateUninitializedArray<byte>((int)entry.Length);
        var memStream = new MemoryStream(outputData, true);
        
        var entryStream = entry.Open();
        entryStream.CopyTo(memStream);
        return outputData;
    }
}