namespace Reloaded.Mod.Loader.Update.Index.Provider;

/// <summary>
/// Package provider for GameBanana that redirects requests through the index (if possible).
/// </summary>
public class IndexedGameBananaPackageProvider : IDownloadablePackageProvider
{
    /// <summary>
    /// ID of the individual game.
    /// </summary>
    public int GameId { get; private set; }

    private bool _initializedApi;
    private IndexPackageProvider _indexPackageProvider;
    private GameBananaPackageProvider _fallback;
    private bool _initializeComplete;

    /// <summary/>
    public IndexedGameBananaPackageProvider(int gameId)
    {
        GameId = gameId;
        _fallback = new GameBananaPackageProvider(gameId);

        _ = InitializeApiAsync();
    }

    private async Task InitializeApiAsync()
    {
        try
        {
            var indexApi = new IndexApi();
            var index    = await indexApi.GetIndexAsync();
            var packages = await index.TryGetGameBananaPackageList(GameId);
            if (packages.result)
            {
                _indexPackageProvider = new IndexPackageProvider(packages.list);
                _initializedApi = true;
            }
        }
        catch (Exception) { /* ignored */ }

        _initializeComplete = true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, SearchOptions? options = null, CancellationToken token = default)
    {
        while (!_initializeComplete)
            await Task.Delay(1, token);

        if (!_initializedApi)
            return await _fallback.SearchAsync(text, skip, take, options, token);

        return await _indexPackageProvider.SearchAsync(text, skip, take, options, token);
    }
}