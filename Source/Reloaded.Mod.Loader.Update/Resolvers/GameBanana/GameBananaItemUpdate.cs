using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Reloaded.Mod.Loader.Update.Resolvers.GameBanana
{
    public class GameBananaItemUpdate
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        [JsonPropertyName("_sTitle")]
        public string Title { get; set; }

        [JsonPropertyName("_aChangeLog")]
        public GameBananaItemUpdateChange[] Changes { get; set; }

        [JsonPropertyName("_sText")]
        public string Text { get; set; }

        [JsonPropertyName("_tsDateAdded")]
        public long DateAddedLong { get; set; }

        [JsonIgnore]
        public DateTime DateAdded => Epoch.AddSeconds(DateAddedLong);
    }
}
