using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Reloaded.Mod.Loader.Community.Utility;

namespace Reloaded.Mod.Loader.Community.Config;

/// <summary>
/// The index file provides a quick lookup for individual game elements.
/// </summary>
public class Index
{
    /// <summary>
    /// Maps a list of individual IDs to matching application profiles.
    /// </summary>
    public Dictionary<string, List<IndexAppEntry>> IdToApps { get; set; } = new Dictionary<string, List<IndexAppEntry>>();

    /// <summary>
    /// Maps a list of hashes to matching games.
    /// </summary>
    public Dictionary<string, List<IndexAppEntry>> HashToAppDictionary { get; set; } = new Dictionary<string, List<IndexAppEntry>>();

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
            var bytes   = File.ReadAllBytes(file);
            var appItem = JsonSerializer.Deserialize<AppItem>(bytes);
            if (string.IsNullOrEmpty(appItem.AppId) || string.IsNullOrEmpty(appItem.Hash))
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
            var newFilePath = Path.Combine(outputFolder, Routes.Application, indexEntry.FilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            File.WriteAllBytes(newFilePath, Compression.Compress(bytes));
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
    /// <returns>List of results.</returns>
    public List<IndexAppEntry> FindApplication(string hash, string id)
    {
        if (HashToAppDictionary.TryGetValue(hash, out var applications)) 
            return applications;

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