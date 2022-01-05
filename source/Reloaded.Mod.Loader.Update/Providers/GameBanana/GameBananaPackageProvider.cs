using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;
using Reloaded.Mod.Loader.Update.Providers.Web;
using Reloaded.Mod.Loader.Update.Utilities;

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
    public async Task<List<IDownloadablePackage>> SearchAsync(string text, int skip = 0, int take = 50, CancellationToken token = default)
    {
        // TODO: Potential bug if no manager integrated mods are returned but there are still more items to take.
        // We ignore it for now but it's best revisited in the future.

        int page       = skip / take;
        var gbApiItems = await GameBananaMod.GetByNameAsync(text, GameId, page, take);
        var results    = new List<IDownloadablePackage>();

        if (gbApiItems == null)
            return results;

        foreach (var result in gbApiItems)
        {
            if (result.ManagerIntegrations == null)
                continue;

            // Calculate authors
            var authors = new List<string>();
            foreach (var creditCategory in result.Credits)
            foreach (var credit in creditCategory.Value)
            {
                authors.Add(credit.Name);
            }

            string author = authors.Count switch
            {
                >= 3 => $"{authors[0]}, {authors[1]}, ...",
                <= 1 => authors[0],
                _ => $"{authors[0]}, {authors[1]}"
            };

            // Check manager integrations.
            int counter = 0;
            foreach (var integratedFile in result.ManagerIntegrations)
            {
                var fileId       = integratedFile.Key;
                var integrations = integratedFile.Value;
                var file         = result.Files.First(x => x.Id == fileId);

                // Build items.
                foreach (var integration in integrations)
                {
                    if (!integration.IsReloadedDownloadUrl().GetValueOrDefault())
                        continue;

                    var url       = new Uri(integration.GetReloadedDownloadUrl());
                    var textDesc  = HtmlUtilities.ConvertToPlainText(result.Description);
                    var downloadFileName = !string.IsNullOrEmpty(file.Description) ? file.Description : file.FileName;
                    var fileName = "";
                    if (counter > 0)
                    {
                        fileName = $"{result.Name!} [{counter++}]";
                    } 
                    else
                    {
                        fileName = $"{result.Name!}";
                        counter++;
                    }

                    results.Add(new WebDownloadablePackage(url, false)
                    {
                        Name = fileName,
                        Description = $"[{downloadFileName}] {textDesc}",
                        Authors = author,
                        FileSize = file.FileSize.GetValueOrDefault(),
                        Source = "GameBanana"
                    });
                }
            }
        }

        return results;
    }
}