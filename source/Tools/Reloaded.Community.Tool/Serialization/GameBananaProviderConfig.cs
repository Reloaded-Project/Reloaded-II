using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Community.Tool.Serialization;

internal class GameBananaProviderConfig : IConfig<GameBananaProviderConfig>
{
    public int GameId { get; set; } = 0;

    // Reflection-less JSON
    public static JsonTypeInfo<GameBananaProviderConfig> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = true;
        return GameBananaProviderConfigContext.Default.GameBananaProviderConfig;
    }
    
    public JsonTypeInfo<GameBananaProviderConfig> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
}

[JsonSerializable(typeof(GameBananaProviderConfig))]
internal partial class GameBananaProviderConfigContext : JsonSerializerContext { }