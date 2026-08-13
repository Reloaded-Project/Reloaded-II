namespace Reloaded.AutoIndexBuilder.Tests;

using Reloaded.AutoIndexBuilder;
using Reloaded.AutoIndexBuilder.Mixin;
using Reloaded.AutoIndexBuilder.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Xunit;

public class DiscordErrorLoggerTests
{
    [Fact]
    public void TruncateDiscordDescription_LeavesShortTextUnchanged()
    {
        const string description = "short message";

        var result = Extensions.TruncateDiscordDescription(description);

        Assert.Equal(description, result);
    }

    [Fact]
    public void TruncateDiscordDescription_ProducesValidDiscordLength()
    {
        var description = new string('x', Extensions.DiscordEmbedDescriptionLimit + 100);

        var result = Extensions.TruncateDiscordDescription(description);

        Assert.Equal(Extensions.DiscordEmbedDescriptionLimit, result.Length);
        Assert.EndsWith("\n...[truncated]", result);
    }

    [Fact]
    public void ErrorSink_DoesNotThrowForOversizedError()
    {
        var sink = new DiscordErrorLoggerSink(_ => Task.CompletedTask, 0);
        var exception = new InvalidOperationException(new string('x', 10_000));
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            exception,
            new MessageTemplateParser().Parse("Build failed"),
            new List<LogEventProperty>());

        var error = Record.Exception(() => sink.Emit(logEvent));

        Assert.Null(error);
    }

    [Fact]
    public async Task ErrorSink_DoesNotThrowWhenDiscordSendFails()
    {
        var sendStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sink = new DiscordErrorLoggerSink(async _ =>
        {
            sendStarted.SetResult();
            await Task.Yield();
            throw new InvalidOperationException("Discord unavailable");
        }, 0);
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Error,
            null,
            new MessageTemplateParser().Parse("Build failed"),
            new List<LogEventProperty>());

        var error = Record.Exception(() => sink.Emit(logEvent));
        await sendStarted.Task;
        await Task.Delay(50);

        Assert.Null(error);
    }
}
