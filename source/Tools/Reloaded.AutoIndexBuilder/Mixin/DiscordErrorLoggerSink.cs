namespace Reloaded.AutoIndexBuilder.Mixin;

/// <summary>
/// Logs errors to Discord, if possible.
/// </summary>
public class DiscordErrorLoggerSink : ILogEventSink
{
    private readonly DiscordSocketClient _client;
    private readonly Settings _settings;

    public DiscordErrorLoggerSink(DiscordSocketClient client, Settings settings)
    {
        _client = client;
        _settings = settings;
    }

    /// <summary>
    /// Sends error logs to Discord.
    /// </summary>
    /// <param name="logEvent"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Error)
            return;

        var embedBuilder = new EmbedBuilder();
        switch (logEvent.Level)
        {
            case LogEventLevel.Error:
                embedBuilder.Color = Color.Red;
                embedBuilder.Description = $"Error!! {logEvent.RenderMessage()}\n" +
                                           $"{logEvent.Exception}\n" +
                                           $"<@{_settings.DiscordOwnerId}>";
                break;
            case LogEventLevel.Fatal:
                embedBuilder.Color = Color.DarkRed;
                embedBuilder.Description = $"Fatal!! {logEvent.RenderMessage()}\n" +
                                           $"{logEvent.Exception}"+
                                           $"<@{_settings.DiscordOwnerId}>";
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!_client.TryGetOutputChannel(_settings, null, nameof(DiscordErrorLoggerSink), out var textChannel))
            return;

        await textChannel!.SendMessageAsync(embed: embedBuilder.Build());
    }
}