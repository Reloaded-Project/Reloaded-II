namespace Reloaded.AutoIndexBuilder.Config;

public class Settings
{
    /// <summary>
    /// Username for Git client.
    /// </summary>
    public string GitUserName { get; set; } = "";

    /// <summary>
    /// Path to the repository.
    /// </summary>
    public string GitRepoPath{ get; set; } = "";

    /// <summary>
    /// Password for Git client.
    /// </summary>
    public string GitPassword { get; set; } = "";

    /// <summary>
    /// Used for reporting to a given text channel.
    /// </summary>
    public string DiscordToken { get; set; } = "";

    /// <summary>
    /// ID of the guild the messages are being output to.
    /// </summary>
    public ulong DiscordGuildId { get; set; } = 0;

    /// <summary>
    /// ID of the person to send error messages to.
    /// </summary>
    public ulong DiscordOwnerId { get; set; } = 0;

    /// <summary>
    /// ID of the channel the message is being output to.
    /// </summary>
    public ulong DiscordChannelId { get; set; } = 0;

    /// <summary>
    /// List of sources to update from.
    /// </summary>
    public List<SourceEntry> Sources { get; set; } = new();

    /// <summary>
    /// Reads the config if available, else creates a dummy and returns null.
    /// </summary>
    public static Settings? TryRead()
    {
        var configPath = Paths.ConfigPath;
        if (File.Exists(configPath))
        {
            var file = File.ReadAllText(configPath);
            var conf = JsonSerializer.Deserialize<Settings>(file)!;
            new SettingsValidator().ValidateAndThrow(conf);
            return conf;
        }

        Console.WriteLine($"Config file not found, writing default config ({configPath}) and exiting.");
        var dummyConfig = new Settings();
        dummyConfig.Sources.Add(new SourceEntry(0));
        File.WriteAllText(configPath, JsonSerializer.Serialize(dummyConfig, new JsonSerializerOptions()
        {
            WriteIndented = true
        }));
        return null;
    }

    /// <summary>
    /// Asynchronously saves the configuration.
    /// </summary>
    public async Task SaveAsync()
    {
        await File.WriteAllTextAsync(Paths.ConfigPath, JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
    }
}