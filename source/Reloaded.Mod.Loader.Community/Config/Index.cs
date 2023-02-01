namespace Reloaded.Mod.Loader.Community.Config;

/// <summary>
/// The index file provides a quick lookup for individual game elements.
/// </summary>
public class Index
{
    /// <summary>
    /// Maps a list of individual IDs to matching application profiles.
    /// </summary>
    public Dictionary<string, List<IndexAppEntry>> IdToApps { get; set; } = new Dictionary<string, List<IndexAppEntry>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps a list of hashes to matching games.
    /// </summary>
    public Dictionary<string, List<IndexAppEntry>> HashToAppDictionary { get; set; } = new Dictionary<string, List<IndexAppEntry>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Builds a game index given a source folder and outputs it to a given directory.
    /// </summary>
    /// <param name="folder">Folder containing the root of the repository, corresponding to root folder of <see cref="Routes"/>.</param>
    /// <param name="outputFolder">Folder where to place the output result.</param>
    /// <returns>A copy of the newly created index.</returns>
    public static Index Build(string folder, string outputFolder)
    {
        folder = Path.GetFullPath(folder);
        outputFolder = Path.GetFullPath(outputFolder);
        Directory.CreateDirectory(outputFolder);

        var applicationFolder = Path.Combine(folder, Routes.Application);
        var files             = Directory.GetFiles(applicationFolder, $"*{Routes.FileExtension}", SearchOption.AllDirectories);
        var result            = new Index();

        foreach (var file in files)
        {
            var appItem = JsonSerializer.Deserialize<AppItem>(File.ReadAllBytes(file));
            if (appItem == null || string.IsNullOrEmpty(appItem.AppId) || string.IsNullOrEmpty(appItem.Hash))
                continue;

            // Make Index Entry
            var relativePath = IO.GetRelativePath(file, applicationFolder);
            var indexEntry   = new IndexAppEntry()
            {
                AppName  = appItem.AppName,
                FilePath = $"{relativePath}{Routes.CompressionExtension}"
            };
            
            GetOrCreateValue(result.IdToApps, appItem.AppId).Add(indexEntry);
            GetOrCreateValue(result.HashToAppDictionary, appItem.Hash).Add(indexEntry);

            // Write new File Out
            var newAppItemBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(appItem)); // Because user made JSON is readable, not indented.
            var newFilePath = Path.Combine(outputFolder, Routes.Application, indexEntry.FilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath)!);
            File.WriteAllBytes(newFilePath, Compression.Compress(newAppItemBytes));
        }

        var indexBytes = Compression.Compress(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));
        File.WriteAllBytes(Path.Combine(outputFolder, Routes.Index), indexBytes);
        
        return result;
    }

    /// <summary>
    /// Searches for an application, at first by hash and by then by id.
    /// </summary>
    /// <param name="hash">Hash of the application to find.</param>
    /// <param name="id">Id of the application to find.</param>
    /// <param name="hashMatches">True if the hash matches, else false.</param>
    /// <returns>List of results.</returns>
    public List<IndexAppEntry> FindApplication(string hash, string id, out bool hashMatches)
    {
        hashMatches = false;
        if (HashToAppDictionary.TryGetValue(hash, out var applications))
        {
            hashMatches = true;
            return applications;
        }

        if (IdToApps.TryGetValue(id, out var files))
            return files;

        return new List<IndexAppEntry>();
    }
    
    private static T2 GetOrCreateValue<T1, T2>(Dictionary<T1, T2> dictionary, T1 key) where T2 : new() where T1 : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
            return value;

        var newValue = new T2();
        dictionary[key] = newValue;
        return newValue;
    }
}

/// <summary>
/// Individual game entry for the index.
/// </summary>
public class IndexAppEntry
{
    /// <summary>
    /// Name of the individual application.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Relative file path on the server for the application.
    /// </summary>
    public string? FilePath { get; set; }
}