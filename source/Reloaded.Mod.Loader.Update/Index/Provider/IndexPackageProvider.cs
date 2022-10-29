namespace Reloaded.Mod.Loader.Update.Index.Provider;

/// <summary>
/// Package provider that returns results from package lists contained in indexes.
/// </summary>
public struct IndexPackageProvider : IDownloadablePackageProvider
{
    private readonly PackageList _packageList;

    /// <summary>
    /// Creates a package provider that uses the saved index for searching mods.
    /// </summary>
    public IndexPackageProvider(PackageList packageList) => _packageList = packageList;

    /// <inheritdoc />
    public Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, SearchOptions? options = null, CancellationToken token = default)
    {
        const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;
        var result = new List<IDownloadablePackage>();
        options ??= new SearchOptions();

        for (var x = skip; x < _packageList.Packages.Count; x++)
        {
            var package = _packageList.Packages[x];
            var name = package.Name;
            if (name.Contains(text, stringComparison))
                result.Add(package);

            if (result.Count >= take)
                return Task.FromResult<IEnumerable<IDownloadablePackage>>(result.ApplyFilters(options.Sort, options.SortDescending));
        }

        return Task.FromResult<IEnumerable<IDownloadablePackage>>(result.ApplyFilters(options.Sort, options.SortDescending));
    }
}