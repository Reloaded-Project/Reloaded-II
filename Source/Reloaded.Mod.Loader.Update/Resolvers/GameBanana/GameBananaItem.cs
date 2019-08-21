using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reloaded.Mod.Loader.Update.Resolvers.GameBanana
{
    /* Disclaimer: Class snippet taken from SADX-Mod-Loader. */
    public class GameBananaItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("Owner().name")]
        public string OwnerName { get; set; }

        [JsonPropertyName("Url().sGetProfileUrl()")]
        public string ProfileUrl { get; set; }

        [JsonPropertyName("Updates().bSubmissionHasUpdates()")]
        public bool HasUpdates { get; set; }

        [JsonPropertyName("Updates().aGetLatestUpdates()")]
        public GameBananaItemUpdate[] Updates { get; set; }

        [JsonPropertyName("Files().aFiles()")]
        public Dictionary<string, GameBananaItemFile> Files { get; set; }

        public static async Task<GameBananaItem> FromTypeAndIdAsync(string itemType, long itemId)
        {
            try
            {
                using (var client = new WebClient())
                {
                    string uriString = $"https://api.gamebanana.com/Core/Item/Data?" +
                                       $"itemtype={itemType}" +
                                       $"&itemid={itemId}" +
                                       $"&fields=name%2C" +
                                       $"Owner().name%2C" +
                                       $"Url().sGetProfileUrl()%2C" +
                                       $"Updates().bSubmissionHasUpdates()%2C" +
                                       $"Updates().aGetLatestUpdates()%2C" +
                                       $"Files().aFiles()&" +
                                       $"return_keys=1";

                    string response = await client.DownloadStringTaskAsync(uriString);
                    return JsonSerializer.Deserialize<GameBananaItem>(response);
                }
            }
            catch
            { return null; }
        }
    }
}
