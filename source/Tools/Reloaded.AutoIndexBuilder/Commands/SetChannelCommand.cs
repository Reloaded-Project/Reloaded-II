namespace Reloaded.AutoIndexBuilder.Commands;

public class SetChannelCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Settings _settings;

    public SetChannelCommand(Settings settings)
    {
        _settings = settings;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("setchannel", "Sets the current channel as the channel the bot reports to.", false, RunMode.Default)]
    public async Task SetChannel()
    {
        var embed = Extensions.MakeSuccessEmbed($"Channel Updated to {Context.Channel.Name}", $"Channel Update Complete");
        _settings.DiscordGuildId = Context.Guild.Id;
        _settings.DiscordChannelId = Context.Channel.Id;
        await RespondAsync(embed: embed);
        await _settings.SaveAsync();
    }
}