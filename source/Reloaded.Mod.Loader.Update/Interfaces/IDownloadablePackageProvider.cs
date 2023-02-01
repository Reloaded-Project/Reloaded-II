namespace Reloaded.Mod.Loader.Update.Interfaces;

/// <summary>
/// Represents an individual package provider which delivers downloadable packages to the user.
/// </summary>
public interface IDownloadablePackageProvider
{
    /// <summary>
    /// Searches for packages matching a given term.
    /// </summary>
    /// <param name="text">The text to search.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take. This is a target. Depending on source, less or more items may be returned.</param>
    /// <param name="options">The options to use in the search, supporting these is opt in, so not all options are supported by all providers.</param>
    /// <param name="token">The token used to cancel the operation.</param>
    public Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, SearchOptions? options = null, CancellationToken token = default);
    
    /// <summary>
    /// Searches for packages matching a given term.
    /// </summary>
    /// <param name="text">The text to search.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take. This is a target. Depending on source, less or more items may be returned.</param>
    /// <param name="token">The token used to cancel the operation.</param>
    public Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default) => SearchAsync(text, skip, take, new SearchOptions(), token);
}

/// <summary>
/// Options used for searching.
/// The options to use in the search, supporting these is opt in, so not all options are supported by all providers.
/// </summary>
public class SearchOptions
{
    /// <summary>
    /// Sorting method applied.
    /// </summary>
    public SearchSorting Sort = SearchSorting.None;

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    public bool SortDescending = true;
}

/// <summary>
/// The sorting applied to the returned results.
/// </summary>
public enum SearchSorting
{
    /// <summary>
    /// No sorting is applied.
    /// </summary>
    None,
    
    /// <summary>
    /// Sort by last modified date.
    /// </summary>
    LastModified,
    
    /// <summary>
    /// Sort by number of downloads.
    /// </summary>
    Downloads,
    
    /// <summary>
    /// Sort by number of likes.
    /// </summary>
    Likes,
    
    /// <summary>
    /// Sort by number of views.
    /// </summary>
    Views
}

/// <summary>
/// Extensions related to downloadable package providers.
/// </summary>
public static class DownloadablePackageProviderExtensions
{
    /// <summary>
    /// Tries to find a package with a matching mod ID.
    /// </summary>
    /// <param name="provider">The package provider to search all items inside.</param>
    /// <param name="modId">The mod ID to search for.</param>
    /// <param name="modName">Name of the mod to search; used when finding by ID does not find wanted package.</param>
    /// <param name="take">The number of items to take in single connection request.</param>
    /// <param name="numConnections">Number of concurrent connections to use.</param>
    /// <param name="alwaysSearchByName">If true, mod will always be searched by name as well as ID, even if ID search returned items.</param>
    /// <param name="token">The token used to cancel the operation.</param>
    public static async Task<List<IDownloadablePackage>> SearchForModAsync(this IDownloadablePackageProvider provider, string modId, string modName, int take = 50, int numConnections = 1, bool alwaysSearchByName = false, CancellationToken token = default)
    {
        var resultsById = await SearchAllAsync(provider, modId, take, numConnections, token);
        var result = resultsById.Where(x => modId.Equals(x.Id)).ToList();
        if (!alwaysSearchByName && result.Count > 0)
            return result;

        var resultsByName = await SearchAllAsync(provider, modName, take, numConnections, token);
        result.AddRange(resultsByName.Where(x => modId.Equals(x.Id)));
        return result;
    }
    
    /// <summary>
    /// Retrieves all packages matching a given term.
    /// </summary>
    /// <param name="provider">The package provider to search all items inside.</param>
    /// <param name="text">The text to search.</param>
    /// <param name="take">The number of items to take in single connection request.</param>
    /// <param name="numConnections">Number of concurrent connections to use.</param>
    /// <param name="token">The token used to cancel the operation.</param>
    public static async Task<IEnumerable<IDownloadablePackage>> SearchAllAsync(this IDownloadablePackageProvider provider, string text, int take = 50, int numConnections = 1, CancellationToken token = default)
    {
        var results = new List<IDownloadablePackage>();
        var paginationHelper = new PaginationHelper();
        paginationHelper.ItemsPerPage = take;
        
        int numResults;
        var searchResults = new Task<IEnumerable<IDownloadablePackage>>[numConnections];
        
        do
        {
            numResults = 0;
            for (int x = 0; x < searchResults.Length; x++)
                searchResults[x] = TrySearch(text, provider, paginationHelper + x);

            await Task.WhenAll(searchResults);

            // Flatten results.
            foreach (var searchResult in searchResults)
            foreach (var downloadablePackage in searchResult.Result)
            {
                numResults += 1;
                results.Add(downloadablePackage);
            }

            paginationHelper.NextPage(numConnections);
        } 
        while (numResults > 0);
        
        return results;
    }
    
    private static async Task<IEnumerable<IDownloadablePackage>> TrySearch(string text, IDownloadablePackageProvider provider, PaginationHelper paginationHelper)
    {
        try
        {
            return await provider.SearchAsync(text, paginationHelper.Skip, paginationHelper.Take, null, default);
        }
        catch (Exception)
        {
            return Array.Empty<IDownloadablePackage>();
        }
    }
}