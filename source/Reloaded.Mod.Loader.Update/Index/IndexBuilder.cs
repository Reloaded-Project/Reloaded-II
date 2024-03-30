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
    /// <param name="writeToFile">Set to true if you wish for the index to be written to file.</param>
    /// <returns>A copy of the newly created index.</returns>
    public async Task<Structures.Index> BuildAsync(string outputFolder, bool writeToFile = true)
    {
        outputFolder  = Path.GetFullPath(outputFolder);
        Directory.CreateDirectory(outputFolder);
        var index     = new Structures.Index();
        index.BaseUrl = new Uri($"{outputFolder}/");
        return await UpdateAsync(index, writeToFile);
    }

    /// <summary>
    /// Removes sources that are not specified in builder.
    /// </summary>
    public Structures.Index RemoveNotInBuilder(Structures.Index index)
    {
        var indexSources = index.Sources.DeepClone();
        
        foreach (var source in Sources)
        {
            switch (source.Type)
            {
                case IndexType.GameBanana:
                    indexSources.Remove(Routes.Source.GetGameBananaIndex(source.GameBananaId!.Value));
                    break;
                case IndexType.NuGet:
                    indexSources.Remove(Routes.Source.GetNuGetIndexKey(source.NuGetUrl!));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var source in indexSources)
        {
            index.Sources.Remove(source.Key);
            var directory = Path.Combine(index.BaseUrl.LocalPath, Path.GetDirectoryName(source.Value)!);
            Directory.Delete(directory, true);
        }

        return index;
    }

    /// <summary>
    /// Updates an existing index instance, by overwriting the data for the configured sources.
    /// </summary>
    /// <param name="index">Existing index instance to be updated.</param>
    /// <param name="writeToFile">Set to true if you wish for the index to be written to file.</param>
    /// <returns>Updated index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<Structures.Index> UpdateAsync(Structures.Index index, bool writeToFile = true)
    {
        var outputFolder = index.BaseUrl.LocalPath;
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
        if (writeToFile)
        {
            await WriteToDiskAsync(index);
            var allPackages = await index.GetPackagesFromAllSourcesAsync();
            await WriteToDiskAsync(index.BaseUrl, allPackages, Routes.AllPackages);
            allPackages.RemoveNonDependencyInfo();
            await WriteToDiskAsync(index.BaseUrl, allPackages, Routes.AllDependencies);
        }

        index.BaseUrl = new Uri(outputFolder, UriKind.Absolute);
        return index;
    }

    /// <summary>
    /// Writes an existing index to disk, in a specified folder.
    /// </summary>
    /// <param name="index">The index to write.</param>
    public async Task WriteToDiskAsync(Structures.Index index)
    {
        var compressedIndex = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(index, Serializer.Options));
        await File.WriteAllBytesAsync(Path.Combine(index.BaseUrl.LocalPath, Routes.Index), compressedIndex);
    }

    /// <summary>
    /// Writes an existing package list to disk, in a specified folder.
    /// </summary>
    /// <param name="list">The list containing all packages.</param>
    /// <param name="baseUrl">The 'base URL' where the Index is contained.</param>
    /// <param name="route">The route where this package list goes.</param>
    public async Task WriteToDiskAsync(Uri baseUrl, PackageList list, string route)
    {
        var compressedPackageList = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(list, Serializer.Options));
        await File.WriteAllBytesAsync(Path.Combine(baseUrl.LocalPath, route), compressedPackageList);
    }

    private async Task BuildNuGetSourceAsync(Structures.Index index, IndexSourceEntry indexSourceEntry,
        string outputFolder)
    {
        // Number of items to grab at once.
        const int take = 500;

        var provider = new NuGetPackageProvider(NugetRepository.FromSourceUrl(indexSourceEntry.NuGetUrl!), null, false);

        var packagesList = PackageList.Create();
        await SearchForAllResults(take, provider, packagesList);

        var relativePath = Routes.Build.GetNuGetPackageListPath(indexSourceEntry.NuGetUrl!);
        var path = Path.Combine(outputFolder, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(packagesList, Serializer.Options));
        await File.WriteAllBytesAsync(path, bytes);
        index.Sources[Routes.Source.GetNuGetIndexKey(indexSourceEntry.NuGetUrl!)] = relativePath;
    }

    private async Task BuildGameBananaSourceAsync(Structures.Index index, IndexSourceEntry indexSourceEntry,
        string outputFolder)
    {
        // Max for GameBanana
        const int take = 50;
        var provider = new GameBananaPackageProvider((int)indexSourceEntry.GameBananaId!.Value);

        var packagesList = PackageList.Create();
        await SearchForAllResults(take, provider, packagesList, 8);

        var relativePath = Routes.Build.GetGameBananaPackageListPath(indexSourceEntry.GameBananaId!.Value);
        var fullPath = Path.Combine(outputFolder, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var bytes = Compression.Compress(JsonSerializer.SerializeToUtf8Bytes(packagesList, Serializer.Options));
        await File.WriteAllBytesAsync(fullPath, bytes);
        index.Sources[Routes.Source.GetGameBananaIndex(indexSourceEntry.GameBananaId!.Value)] = relativePath;
    }

    private static async Task SearchForAllResults(int itemsPerPage, IDownloadablePackageProvider provider, PackageList packagesList, int numConnections = 1)
    {
        var paginationHelper = new PaginationHelper();
        paginationHelper.ItemsPerPage = itemsPerPage;

        int numResults;
        var searchResults = new Task<IEnumerable<IDownloadablePackage>>[numConnections];

        do
        {
            numResults = 0;
            for (int x = 0; x < searchResults.Length; x++)
                searchResults[x] = TrySearch(provider, paginationHelper + x);

            await Task.WhenAll(searchResults);

            // Flatten results.
            foreach (var searchResult in searchResults)
            foreach (var downloadablePackage in searchResult.Result)
            {
                numResults += 1;
                packagesList.Packages.Add(await Package.CreateAsync(downloadablePackage));
            }

            paginationHelper.NextPage(numConnections);
        } while (numResults > 0);
    }

    private static async Task<IEnumerable<IDownloadablePackage>> TrySearch(IDownloadablePackageProvider provider, PaginationHelper paginationHelper)
    {
        try
        {
            return await provider.SearchAsync("", paginationHelper.Skip, paginationHelper.Take, null, default);
        }
        catch (Exception)
        {
            // TODO: Log error
            return Array.Empty<IDownloadablePackage>();
        }
    }
}