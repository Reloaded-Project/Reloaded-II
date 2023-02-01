namespace Reloaded.AutoIndexBuilder.Commands;

public class ForceRunCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IndexBuilderService _builder;

    public ForceRunCommand(IndexBuilderService builder)
    {
        _builder = builder;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("forcerun", "Force runs all index updates without waiting for next refresh cycle.", false, RunMode.Async)]
    public async Task SetChannel()
    {
        await RespondAsync(embed: Extensions.MakeSuccessEmbed($"Starting all updates, hang tight!", "Started Running Updates!"));
        await _builder.ForceRunAsync();
    }
}