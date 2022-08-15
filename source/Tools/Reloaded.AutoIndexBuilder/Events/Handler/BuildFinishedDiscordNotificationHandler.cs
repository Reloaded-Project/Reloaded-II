namespace Reloaded.AutoIndexBuilder.Events.Handler;

/// <summary>
/// Prints embed with build statistics.
/// </summary>
public class BuildFinishedDiscordNotificationHandler : INotificationHandler<BuildFinishedNotification>
{
    private readonly DiscordSocketClient _client;
    private readonly Settings _settings;
    private readonly Logger _logger;

    public BuildFinishedDiscordNotificationHandler(DiscordSocketClient client, Settings settings, Logger logger)
    {
        _client = client;
        _settings = settings;
        _logger = logger;
    }

    public async Task Handle(BuildFinishedNotification notification, CancellationToken cancellationToken)
    {
        if (!_client.TryGetOutputChannel(_settings, _logger, nameof(BuildFinishedDiscordNotificationHandler), out var channel))
            return;

        _logger.Information("Build {@request} Completed", notification);
        var entry = notification.Entry;
        var embed = Extensions.MakeSuccessEmbed($"Build '{entry.FriendlyName}' completed in, {notification.Runtime.TotalSeconds:0.00} seconds.\n" +
                                                $"Next build in: {entry.MinutesBetweenRefresh} minute(s).", "Build Completed!");

        await channel!.SendMessageAsync(embed: embed);
    }
}