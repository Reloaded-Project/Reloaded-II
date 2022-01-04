using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Update.Providers.GameBanana;

/// <summary>
/// Provider that allows for searching of downloadable mods on GameBanana.
/// </summary>
public class GameBananaPackageProvider : IDownloadablePackageProvider
{
    /// <summary>
    /// ID of the individual game.
    /// </summary>
    public int GameId { get; private set; }

    /// <summary/>
    public GameBananaPackageProvider(int gameId)
    {
        GameId = gameId;
    }

    /// <inheritdoc />
    public Task<List<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}