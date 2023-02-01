namespace Reloaded.AutoIndexBuilder.Commands;

public class ReportUptimeCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Stats _stats;

    public ReportUptimeCommand(Stats stats)
    {
        _stats = stats;
    }
    
    [SlashCommand("stats", "Retrieves the current stats of the bot.", false, RunMode.Default)]
    public async Task Report()
    {
        var embed = Extensions.MakeInfoEmbed($"Started: <t:{_stats.StartTimeUnix}:R>\n" +
                                             $"Total Builds: {_stats.TotalBuilds}\n" +
                                             $"Builds Since Started: {_stats.BuildsSinceStarted}", "Current Stats");

        await RespondAsync(embed: embed);
    }
}