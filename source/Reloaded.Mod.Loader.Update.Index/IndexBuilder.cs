using System;
using System.IO;
using System.Security.Policy;
using System.Text.Json;
using Reloaded.Mod.Loader.IO.Config.Structs;
using Reloaded.Mod.Loader.Update.Index.Structures;
using Reloaded.Mod.Loader.Update.Index.Structures.Config;
using Reloaded.Mod.Loader.Update.Index.Utility;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Reloaded.Mod.Loader.Update.Providers.NuGet;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;

namespace Reloaded.Mod.Loader.Update.Index;

/// <summary>
/// Builds the index.
/// </summary>
public class IndexBuilder
{
    /// <summary>
    /// List of sources to build index from.
    /// </summary>
    public List<IndexSourceEntry> Sources { get; set; } = new();

    /// <summary>
    /// Builds a game index given a source folder and outputs it to a given directory.
    /// </summary>
    /// <param name="outputFolder">Folder where to place the output result.</param>
    /// <returns>A copy of the newly created index.</returns>
    public async Task<Structures.Index> BuildAsync(string outputFolder)
    {
        outputFolder = Path.GetFullPath(outputFolder);
        Directory.CreateDirectory(outputFolder);

        var index = new Structures.Index();
        var tasks = new Task[Sources.Count];
        for (var x = 0; x < Sources.Count; x++)
        {
            var source = Sources[x];
            switch (source.Type)
            {
                case IndexType.GameBanana:
                    tasks[x] = BuildGameBananaSourceAsync(index, source, outputFolder);
                    break;
                case IndexType.NuGet:
                    tasks[x] = BuildNuGetSourceAsync(index, source, outputFolder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await Task.WhenAll(tasks);
        var compressedIndex = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(index, Serializer.Options));
        await File.WriteAllBytesAsync(Path.Combine(outputFolder, Routes.Index), compressedIndex);
        return index;
    }

    private async Task BuildNuGetSourceAsync(Structures.Index index, IndexSourceEntry indexSourceEntry,
        string outputFolder)
    {
        // Number of items to grab at once.
        const int Take = 500;

        var provider = new NuGetPackageProvider(new AggregateNugetRepository(new []
        {
            new NugetFeed("Cool Feed", indexSourceEntry.NuGetUrl!)
        }));

        var packagesList = new PackageList();
        await SearchForAllResults(Take, provider, packagesList);

        var path = Path.Combine(outputFolder, Routes.Build.GetNuGetPackageListPath(indexSourceEntry.NuGetUrl!));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(packagesList, Serializer.Options));
        await File.WriteAllBytesAsync(path, bytes);
        index.Sources[Routes.Source.GetNuGetIndexKey(indexSourceEntry.NuGetUrl!)] = path;
    }

    private static async Task SearchForAllResults(int Take, IDownloadablePackageProvider provider, PackageList packagesList)
    {
        var paginationHelper = new PaginationHelper();
        paginationHelper.ItemsPerPage = Take;

        IDownloadablePackage[] resultsArray;

        do
        {
            resultsArray = (await provider.SearchAsync("", paginationHelper.Skip, paginationHelper.Take)).ToArray();
            foreach (var result in resultsArray)
                packagesList.Packages.Add(await Package.CreateAsync(result));

            paginationHelper.NextPage();
        } while (resultsArray.Length > 0);
    }

    private async Task BuildGameBananaSourceAsync(Structures.Index index, IndexSourceEntry indexSourceEntry, string outputFolder)
    {
        // Max for GameBanana
        const int Take = 50;
        var provider = new GameBananaPackageProvider((int)indexSourceEntry.GameBananaId!.Value);

        var packagesList = new PackageList();
        await SearchForAllResults(Take, provider, packagesList);
        
        var path = Path.Combine(outputFolder, Routes.Build.GetGameBananaPackageListPath(indexSourceEntry.GameBananaId!.Value));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(packagesList, Serializer.Options));
        await File.WriteAllBytesAsync(path, bytes);
        index.Sources[Routes.Source.GetGameBananaIndex(indexSourceEntry.GameBananaId!.Value)] = path;
    }
}