using Environment = Reloaded.Mod.Shared.Environment;

namespace Reloaded.Mod.Loader.Logging;

/// <summary>
/// Prints the Reloaded banner to the given console.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ReloadedBannerLogger
{
    public static void PrintBanner(IConsoleProxy proxy, ILogger logger)
    {
        proxy.Write("\n\n");

        var lines = new[]
        {
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMMMMMMMMMMMMMMMMdo`    ", logger.TextColor), new FormattedLineSegment("    hMMM+ +MMMh", logger.ColorRed) }},
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMMMMMMMMMMMMMMMMMMh    ", logger.TextColor), new FormattedLineSegment("   `MMMM` dMMM/", logger.ColorRed) }}, 
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-          `yMMMN    ", logger.TextColor), new FormattedLineSegment("   +MMMh .MMMN`", logger.ColorRed) }}, 
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-          `sMMMN    ", logger.TextColor), new FormattedLineSegment("   dMMM/ sMMMy ", logger.ColorRed) }},
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM- .sMMMMMMMMMMMMd    ", logger.TextColor), new FormattedLineSegment("  -MMMN  NMMM: ", logger.ColorRed) }}, 
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-   `sNMMMMMMMdo`    ", logger.TextColor), new FormattedLineSegment("  sMMMy :MMMm  ", logger.ColorRed) }}, 
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-     `oNMMMMy-      ", logger.TextColor), new FormattedLineSegment("  NMMM- yMMMo  ", logger.ColorRed) }},
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-       `oNMMMMh-    ", logger.TextColor), new FormattedLineSegment(" :MMMm `MMMM.  ", logger.ColorRed) }}, 
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-         `+mMMMMh:  ", logger.TextColor), new FormattedLineSegment(" yMMMo /MMMd   ", logger.ColorRed) }},
            new FormattedLine() { Segments = new []{ new FormattedLineSegment("MMMM-            /mMMMMd:", logger.TextColor), new FormattedLineSegment("`MMMM. dMMM+   ", logger.ColorRed) }}, 
        };

        WriteLinesCentered(proxy, lines);
        proxy.Write("\n");
        PrintCoreVersion(proxy, logger);
        proxy.Write("\n\n");
    }

    private static void WriteLinesCentered(IConsoleProxy proxy, FormattedLine[] lines)
    {
        foreach (var line in lines)
            WriteLineCentered(proxy, line);
    }

    private static void WriteLineCentered(IConsoleProxy proxy, FormattedLine line)
    {
        CenterCursor(proxy, line.GetLength());
        line.WriteLine(proxy);
    }

    private static void CenterCursor(IConsoleProxy proxy, int textLength)
    {
        // Get center, accounting for overflow.
        int consolePointer = (System.Console.WindowWidth - textLength) / 2;
        if (consolePointer < 0)
            consolePointer = 0;

        proxy.SetCursorPosition(consolePointer, System.Console.CursorTop);
    }

    private static void PrintCoreVersion(IConsoleProxy proxy, ILogger logger)
    {
        var version = Assembly.GetExecutingAssembly()!.GetName().Version.ToString(3);
        var coreVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        if (Environment.IsWine)
            coreVersion += " (via Wine)";

        WriteLineCentered(proxy, new FormattedLine()
        {
            Segments = new []{ new FormattedLineSegment($"{version} // ", logger.TextColor), new FormattedLineSegment($"{coreVersion}", logger.ColorRed) }
        });
    }

    private struct FormattedLine
    {
        public FormattedLineSegment[] Segments;
        public int GetLength() => Segments.Sum(x => x.Text.Length);
        public void WriteLine(IConsoleProxy proxy)
        {
            foreach (var segment in Segments)
                proxy.Write(segment.Text, segment.Color);

            proxy.Write("\n");
        }
    }

    private struct FormattedLineSegment
    {
        public string Text;
        public Color Color;

        public FormattedLineSegment(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }
}