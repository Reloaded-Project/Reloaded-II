using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Reloaded.Mod.Loader.Update.Resolvers.GameBanana
{
    public class GameBananaItemUpdateChange
    {
        [JsonPropertyName("cat")]
        public string Category { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
