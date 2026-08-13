namespace Reloaded.AutoIndexBuilder.Utilities;

public static class Extensions
{
    /// <summary>
    /// Maximum length of an embed description accepted by Discord.
    /// </summary>
    public const int DiscordEmbedDescriptionLimit = 4096;

    /// <summary>
    /// Truncates text to Discord's embed description limit.
    /// </summary>
    public static string TruncateDiscordDescription(string? description)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= DiscordEmbedDescriptionLimit)
            return description ?? string.Empty;

        const string suffix = "\n...[truncated]";
        return description[..(DiscordEmbedDescriptionLimit - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Makes a Discord error embed for a given title and description.
    /// </summary>
    public static Embed MakeInfoEmbed(string description, string title = "Into")
    {
        return new EmbedBuilder()
            .WithColor(Color.LightGrey)
            .WithTitle(title)
            .WithDescription(TruncateDiscordDescription(description))
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }

    /// <summary>
    /// Makes a Discord error embed for a given title and description.
    /// </summary>
    public static Embed MakeErrorEmbed(string description, string title = "Error")
    {
        return new EmbedBuilder()
            .WithColor(Color.Red)
            .WithTitle(title)
            .WithDescription(TruncateDiscordDescription(description))
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }

    /// <summary>
    /// Makes a Discord success embed for a given title and description.
    /// </summary>
    public static Embed MakeSuccessEmbed(string description, string title = "Success!")
    {
        return new EmbedBuilder()
            .WithColor(Color.Green)
            .WithTitle(title)
            .WithDescription(TruncateDiscordDescription(description))
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }

    /// <summary>
    /// Makes a Discord success embed for a given title and description.
    /// </summary>
    public static Embed MakeWarningEmbed(string description, string title = "Warning!")
    {
        return new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle(title)
            .WithDescription(TruncateDiscordDescription(description))
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }

    /// <summary>
    /// Gets the channel that the discord client should output to.
    /// </summary>
    /// <param name="client">The discord client used.</param>
    /// <param name="settings">Settings of the application.</param>
    /// <param name="logger">The logger used for logging.</param>
    /// <param name="source">The source channel to output to.</param>
    /// <param name="channel">The text channel to send messages to.</param>
    /// <returns></returns>
    public static bool TryGetOutputChannel(this DiscordSocketClient client, Settings settings, Logger? logger, string source, out ITextChannel? channel)
    {
        // Get discord guild & channel.
        if (settings.DiscordGuildId == 0 || settings.DiscordChannelId == 0)
        {
            logger?.Warning("{0} Discord Channel is not Set.", source);
            channel = null;
            return false;
        }

        var guild = client.GetGuild(settings.DiscordGuildId);
        if (guild == null)
        {
            logger?.Error($"{nameof(BuildFinishedDiscordNotificationHandler)} Discord Guild Not Found.");
            channel = null;
            return false;
        }

        channel = guild.GetTextChannel(settings.DiscordChannelId);
        if (channel == null)
        {
            logger?.Error($"{nameof(BuildFinishedDiscordNotificationHandler)} Discord Channel Not Found.");
            channel = null;
            return false;
        }
        
        return true;
    }

}
