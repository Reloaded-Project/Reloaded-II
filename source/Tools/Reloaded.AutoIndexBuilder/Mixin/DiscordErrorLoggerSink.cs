namespace Reloaded.AutoIndexBuilder.Mixin;

/// <summary>
/// Logs errors to Discord, if possible.
/// </summary>
public class DiscordErrorLoggerSink : ILogEventSink
{
    private readonly Func<Embed, Task> _sendMessageAsync;
    private readonly ulong _discordOwnerId;

    public DiscordErrorLoggerSink(Func<Embed, Task> sendMessageAsync, ulong discordOwnerId)
    {
        _sendMessageAsync = sendMessageAsync;
        _discordOwnerId = discordOwnerId;
    }

    /// <summary>
    /// Sends error logs to Discord.
    /// </summary>
    /// <param name="logEvent"></param>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Error)
            return;

        try
        {
            var embedBuilder = new EmbedBuilder
            {
                Color = logEvent.Level == LogEventLevel.Fatal ? Color.DarkRed : Color.Red,
                Description = Extensions.TruncateDiscordDescription(
                    $"{(logEvent.Level == LogEventLevel.Fatal ? "Fatal" : "Error")}!! " +
                    $"{logEvent.RenderMessage()}\n" +
                    $"{logEvent.Exception}\n" +
                    $"<@{_discordOwnerId}>")
            };

            // After this line: Discord errors cannot escape logging sink.
            _ = SendMessageSafelyAsync(embedBuilder.Build());
        }
        catch (Exception exception)
        {
            ReportSinkFailure(exception);
        }
    }

    private async Task SendMessageSafelyAsync(Embed embed)
    {
        try
        {
            await _sendMessageAsync(embed);
        }
        catch (Exception exception)
        {
            ReportSinkFailure(exception);
        }
    }

    private static void ReportSinkFailure(Exception exception)
    {
        // Do not use Serilog here. This is failure handling for Serilog itself.
        try
        {
            Console.Error.WriteLine($"Discord error logger failed: {exception}");
        }
        catch
        {
            // Logging must never terminate the worker process.
        }
    }
}
