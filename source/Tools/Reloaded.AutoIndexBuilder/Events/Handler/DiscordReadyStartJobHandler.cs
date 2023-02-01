namespace Reloaded.AutoIndexBuilder.Events.Handler;

internal class DiscordReadyStartJobHandler : INotificationHandler<DiscordReadyNotification>
{
    private readonly DiscordSocketClient _client;
    private readonly IndexBuilderService _indexBuilder;
    private readonly Settings _settings;
    private readonly Logger _logger;

    public DiscordReadyStartJobHandler(DiscordSocketClient client, IndexBuilderService indexBuilder, Settings settings, Logger logger)
    {
        _client = client;
        _indexBuilder = indexBuilder;
        _settings = settings;
        _logger = logger;
    }

    public async Task Handle(DiscordReadyNotification notification, CancellationToken cancellationToken)
    {
        if (_client.TryGetOutputChannel(_settings, _logger, nameof(DiscordReadyStartJobHandler), out var channel))
            await WriteReadyMessage(channel!);

        await _indexBuilder.InitAsync();
    }

    private async Task WriteReadyMessage(ITextChannel channel)
    {
        await channel.SendMessageAsync(embed: Extensions.MakeSuccessEmbed($"Bot Started at Time {DateTimeOffset.UtcNow}."));
    }
}