using System.Net.Http;
using CacheCow.Client;
using Reloaded.Mod.Loader.Update.Caching;

#pragma warning disable CS1591

namespace Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;

/// <summary>
/// Represents an individual GameBanana mod entry.
/// </summary>

public class GameBananaMod
{
    [JsonPropertyName("_idRow")]
    public long Id { get; set; }

    [JsonPropertyName("_sName")]
    public string? Name { get; set; }

    [JsonPropertyName("_sText")]
    public string? Description { get; set; }

    [JsonPropertyName("_sProfileUrl")]
    public string LinkToModPage { get; set; } = "";

    [JsonPropertyName("_aFiles")]
    public List<GameBananaModFile>? Files { get; set; }

    [JsonPropertyName("_aCredits")]
    public Dictionary<string, GameBananaCredit[]>? Credits { get; set; }

    [JsonPropertyName("_aPreviewMedia")]
    public GameBananaPreviewMedia? PreviewMedia { get; set; }

    [JsonPropertyName("_aSubmitter")]
    public GameBananaSubmitter Submitter { get; set; } = new GameBananaSubmitter();

    [JsonPropertyName("_nDownloadCount")]
    public long DownloadCount { get; set; }

    [JsonPropertyName("_nViewCount")]
    public long ViewCount { get; set; }

    [JsonPropertyName("_nLikeCount")]
    public long LikeCount { get; set; }

    [JsonConverter(typeof(GameBananaManagerIntegrationConverter))]
    [JsonPropertyName("_aModManagerIntegrations")]
    public Dictionary<string, GameBananaManagerIntegration[]>? ManagerIntegrations { get; set; }

    /// <summary>
    /// Makes a web request to get mod details given a set of different details.
    /// Uses APIv7.
    /// </summary>
    /// <param name="searchText">The text to search.</param>
    /// <param name="gameId">Game identifier.</param>
    /// <param name="page">Page (for pagination). ONE INDEXED</param>
    /// <param name="take">Number of items to take.</param>
    public static async Task<List<GameBananaMod>?> GetByNameAsync(string? searchText, int gameId, int page, int take)
    {
        try
        {
            // Note: Page is 1 indexed.
            string urlString = "";

            // Transform search text to include wildcards.
            if (searchText != null && searchText.Trim().Length >= 1)
            {
                searchText = $"*{searchText}*";
                urlString = $"https://gamebanana.com/apiv7/Mod/ByName?" +
                            $"_nPerpage={take}&" +
                            $"_nPage={page}&" +
                            $"_csvProperties=_idRow,_sName,_aFiles,_aCredits,_aModManagerIntegrations,_sText,_aPreviewMedia,_aSubmitter,_sProfileUrl,_nViewCount,_nLikeCount,_nDownloadCount&" +
                            $"_sName={searchText}&" +
                            $"_idGameRow={gameId}";
            }
            else
            {
                urlString = $"https://gamebanana.com/apiv7/Mod/ByGame?" +
                            $"_nPerpage={take}&" +
                            $"_nPage={page}&" +
                            $"_csvProperties=_idRow,_sName,_aFiles,_aCredits,_aModManagerIntegrations,_sText,_aPreviewMedia,_aSubmitter,_sProfileUrl,_nViewCount,_nLikeCount,_nDownloadCount&" +
                            $"_aGameRowIds[]={gameId}";
            }

            string response = await SharedHttpClient.UncachedAndCompressed.GetStringAsync(urlString);
            return JsonSerializer.Deserialize<List<GameBananaMod>>(response, new JsonSerializerOptions()
            {
                Converters = { GameBananaCredit.GameBananaCreditJsonConverter.Instance }
            });
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Includes the details of previewable media such as images.
/// </summary>
[ExcludeFromCodeCoverage]
public class GameBananaPreviewMedia
{
    /// <summary>Images for this submission.</summary>
    [JsonPropertyName("_aImages")]
    public GameBananaPreviewImage[]? Images { get; set; }
}

/// <summary>
/// Includes the details of an preview image.
/// </summary>
[ExcludeFromCodeCoverage]
public class GameBananaPreviewImage
{
    /// <summary>
    /// Maximum number of thumbnails supported.
    /// </summary>
    public const int MaxThumbnailCount = 3;

    /// <summary> e.g. 'screenshot' </summary>
    [JsonPropertyName("_sType")]
    public string Type { get; set; } = null!;

    /// <summary> e.g. 'https://images.gamebanana.com/img/ss/mods' </summary>
    [JsonPropertyName("_sBaseUrl")]
    public string BaseUrl { get; set; } = null!;

    /// <summary> e.g. 'SH title in Widescreen baby' </summary>
    [JsonPropertyName("_sCaption")]
    public string? Caption { get; set; }

    /// <summary> e.g. '62646971886ef.jpg' </summary>
    [JsonPropertyName("_sFile")]
    public string File { get; set; } = null!;

    /// <summary> e.g. '220-90_62646971886ef.jpg"' </summary>
    [JsonPropertyName("_sFile220")]
    public string? FileWidth220 { get; set; }

    /// <summary> e.g. '530-90_62646971886ef.jpg"' </summary>
    [JsonPropertyName("_sFile530")]
    public string? FileWidth530 { get; set; }

    /// <summary> e.g. '100-90_62646971886ef.jpg"' </summary>
    [JsonPropertyName("_sFile100")]
    public string? FileWidth100 { get; set; }
}

/// <summary>
/// Represents an individual mod uploader on GameBanana
/// _csvProperties = _aSubmitter
/// </summary>
[ExcludeFromCodeCoverage]
public class GameBananaCredit
{
    [JsonPropertyName("")]
    public string[]? Values { get; set; }

    [JsonIgnore]
    public string? Name => Values?[0];

    [JsonIgnore]
    public string? Role => Values?[1];

    public class GameBananaCreditJsonConverter : JsonConverter<GameBananaCredit>
    {
        public static GameBananaCreditJsonConverter Instance { get; } = new GameBananaCreditJsonConverter();

        public override GameBananaCredit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Start of array.
            reader.Read();
            var values = new List<string>();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                values.Add(reader.GetString()!);
                reader.Read();
            }

            return new GameBananaCredit()
            {
                Values = values.ToArray()
            };
        }

        public override void Write(Utf8JsonWriter writer, GameBananaCredit value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.Name);
            writer.WriteStringValue(value.Role);
            writer.WriteEndArray();
        }
    }
}

/// <summary>
/// Includes the details of an individual mod manager integration.
/// </summary>
[ExcludeFromCodeCoverage]
public class GameBananaManagerIntegration
{
    // See: Reloaded.Mod.Launcher.Lib.Misc
    public const string ReloadedProtocol = "R2";

    [JsonPropertyName("_sInstallerName")]
    public string? InstallerName { get; set; }

    [JsonPropertyName("_sInstallerUrl")]
    public string? InstallerUrl { get; set; }

    [JsonPropertyName("_sIconClasses")]
    public string? IconClass { get; set; }

    [JsonPropertyName("_sDownloadUrl")]
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Returns true if the download URL is a Reloaded URL.
    /// </summary>
    public bool? IsReloadedDownloadUrl() => DownloadUrl?.StartsWith(ReloadedProtocol, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the download URL with the reloaded specifier stripped out.
    /// </summary>
    public string GetReloadedDownloadUrl() => DownloadUrl!.Substring(ReloadedProtocol.Length + 1);
}

/// <summary>
/// Represents an individual mod uploader on GameBanana
/// _csvProperties = _aSubmitter
/// </summary>
[ExcludeFromCodeCoverage]
public class GameBananaSubmitter
{
    [JsonPropertyName("_idRow")]
    public long Id { get; set; } = default;

    [JsonPropertyName("_sName")]
    public string Name { get; set; } = "";

    [JsonPropertyName("_sUserTitle")]
    public string? UserTitle { get; set; }

    [JsonPropertyName("_sHonoraryTitle")]
    public string? HonoraryTitle { get; set; }

    [JsonPropertyName("_tsJoinDate")]
    public long? JoinDateLong { get; set; }

    [JsonIgnore]
    public DateTime JoinDate => DateTime.UnixEpoch.AddSeconds(JoinDateLong.GetValueOrDefault());

    [JsonPropertyName("_sAvatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("_sSigUrl")]
    public string? SignatureUrl { get; set; }

    [JsonPropertyName("_sProfileUrl")]
    public string ProfileUrl { get; set; } = "";

    [JsonPropertyName("_sUpicUrl")]
    public string? UberPicUrl { get; set; }

    [JsonPropertyName("_sPointsUrl")]
    public string? PointsUrl { get; set; }

    [JsonPropertyName("_sMedalsUrl")]
    public string? MedalsUrl { get; set; }

    [JsonPropertyName("_bIsOnline")]
    public bool? IsOnline { get; set; }

    [JsonPropertyName("_sLocation")]
    public string? Location { get; set; }

    [JsonPropertyName("_sOnlineTitle")]
    public string? OnlineTitle { get; set; }

    [JsonPropertyName("_sOfflineTitle")]
    public string? OfflineTitle { get; set; }

    [JsonPropertyName("_nPoints")]
    public long? Points { get; set; }

    [JsonPropertyName("_nPointsRank")]
    public long? PointsRank { get; set; }

    // Medals: Since API docs are not public, I dunno the field names, don't wanna go around figuring them out.

    [JsonPropertyName("_bHasRipe")]
    public bool? HasRipe { get; set; }

    [JsonPropertyName("_sSubjectShaperCssCode")]
    public string? SubjectShaperCssCode { get; set; }

    [JsonPropertyName("_sCooltipCssCode")]
    public string? CooltipCssCode { get; set; }

    [JsonPropertyName("_nBuddyCount")]
    public long? BuddyCount { get; set; }

    [JsonPropertyName("_nSubscriberCount")]
    public long? SubscriberCount { get; set; }
}

/// <summary>
/// Represents an individual downloadable file for GameBanana mods.
/// </summary>

[ExcludeFromCodeCoverage]
public class GameBananaModFile
{
    // Note: This is a number in apiv8
    [JsonPropertyName("_idRow")]
    public string Id { get; set; } = "";

    [JsonPropertyName("_sFile")]
    public string FileName { get; set; } = null!;

    [JsonPropertyName("_nFilesize")]
    public long? FileSize { get; set; }

    [JsonPropertyName("_bIsMissing")]
    public bool? IsMissing { get; set; }

    [JsonPropertyName("_sDescription")]
    public string? Description { get; set; }

    [JsonPropertyName("_tsDateAdded")]
    public long? DateAddedLong { get; set; }

    [JsonPropertyName("_sAnalysisCode")]
    public string? AnalysisCode { get; set; }

    [JsonPropertyName("_sAnalysisResult")]
    public string? AnalysisResult { get; set; }

    [JsonPropertyName("_nDownloadCount")]
    public long? DownloadCount { get; set; }

    [JsonPropertyName("_sDownloadUrl")]
    public string? DownloadUrl { get; set; }

    [JsonIgnore]
    public DateTime DateAdded => DateTime.UnixEpoch.AddSeconds(DateAddedLong.GetValueOrDefault());
}

public class GameBananaManagerIntegrationConverter : JsonConverter<Dictionary<string, GameBananaManagerIntegration[]>>
{
    private JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public override Dictionary<string, GameBananaManagerIntegration[]>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            do { reader.Read(); } 
            while (reader.TokenType != JsonTokenType.EndArray);
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, GameBananaManagerIntegration[]>>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, GameBananaManagerIntegration[]> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, _options);
    }
}