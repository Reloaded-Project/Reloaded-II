using System;
using System.Drawing;
using Colorful;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Logging.Init;

namespace Reloaded.Mod.Loader.Logging
{
    public class Console : ILogger
    {
        public event EventHandler<string> OnPrintMessage = (sender, s) => { };

        /* Default constructor */
        public Console() { }

        public void ShowConsole()
        {
            ConsoleAllocator.Alloc();

            Colorful.Console.BackgroundColor = BackgroundColor;
            Colorful.Console.ForegroundColor = TextColor;
            System.Console.Clear();
            PrintBanner();
        }

        public void PrintMessage(string message, Color color)   => WriteLine(message, color);
        public void WriteLine(string message)                   => WriteLine(message, TextColor);
        public void Write(string message)                       => Write(message, TextColor);

        public void WriteLine(string message, Color color)
        {
            Colorful.Console.WriteLine(message, color);
            OnPrintMessage?.Invoke(this, message);
        }

        public void Write(string message, Color color)
        {
            Colorful.Console.Write(message, color);
            OnPrintMessage?.Invoke(this, message);
        }

        // Default Colours
        public Color BackgroundColor    { get; set; } = Color.FromArgb(20, 25, 31);
        public Color TextColor          { get; set; } = Color.FromArgb(239, 240, 235);

        public Color ColorRed           { get; set; } = Color.FromArgb(255, 92, 87);
        public Color ColorRedLight      { get; set; } = Color.FromArgb(220, 163, 163);

        public Color ColorGreen         { get; set; } = Color.FromArgb(90, 247, 142);
        public Color ColorGreenLight    { get; set; } = Color.FromArgb(195, 191, 159);

        public Color ColorYellow        { get; set; } = Color.FromArgb(243, 249, 157);
        public Color ColorYellowLight   { get; set; } = Color.FromArgb(240, 223, 175);

        public Color ColorBlue          { get; set; } = Color.FromArgb(87, 199, 255);
        public Color ColorBlueLight     { get; set; } = Color.FromArgb(148, 191, 243);

        public Color ColorPink          { get; set; } = Color.FromArgb(255, 106, 193);
        public Color ColorPinkLight     { get; set; } = Color.FromArgb(236, 147, 211);

        public Color ColorLightBlue     { get; set; } = Color.FromArgb(154, 237, 254);
        public Color ColorLightBlueLight { get; set; } = Color.FromArgb(147, 224, 227);

        /* Default Banner */
        private void PrintBanner()
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("");

            Formatter[] formatter = {
                new Formatter(@"    hMMM+ +MMMh", ColorRed),
                new Formatter(@"   `MMMM` dMMM/", ColorRed),
                new Formatter(@"   +MMMh .MMMN`", ColorRed),
                new Formatter(@"   dMMM/ sMMMy ", ColorRed),
                new Formatter(@"  -MMMN  NMMM: ", ColorRed),
                new Formatter(@"  sMMMy :MMMm  ", ColorRed),
                new Formatter(@"  NMMM- yMMMo  ", ColorRed),
                new Formatter(@" :MMMm `MMMM.  ", ColorRed),
                new Formatter(@" yMMMo /MMMd   ", ColorRed),
                new Formatter(@"`MMMM. dMMM+   ", ColorRed),

            };

            string[] template = new[]
            {
                @"MMMMMMMMMMMMMMMMMMdo`    {0}",
                @"MMMMMMMMMMMMMMMMMMMMh    {0}",
                @"MMMM-          `yMMMN    {0}",
                @"MMMM-          `sMMMN    {0}",
                @"MMMM- .sMMMMMMMMMMMMd    {0}",
                @"MMMM-   `sNMMMMMMMdo`    {0}",
                @"MMMM-     `oNMMMMy-      {0}",
                @"MMMM-       `oNMMMMh-    {0}",
                @"MMMM-         `+mMMMMh:  {0}",
                @"MMMM-            /mMMMMd:{0}"
            };


            // Hardcoded for this banner, please do not reuse.
            // -3 from the templated {0}
            WriteLinesCentered(template, formatter, -3);

            System.Console.WriteLine("");
            System.Console.WriteLine("");
        }

        private void WriteLinesCentered(string[] lines, Formatter[] formatters, int characterOffset)
        {
            // Hardcoded for this banner, please do not reuse.
            for (int x = 0; x < lines.Length; x++)
            {
                var line        = lines[x];
                var formatter   = formatters[x];

                // Get center, accounting for overflow.
                int consolePointer = (System.Console.WindowWidth - line.Length + characterOffset) / 2;
                if (consolePointer < 0)
                    consolePointer = 0;

                System.Console.SetCursorPosition(consolePointer, System.Console.CursorTop);
                Colorful.Console.WriteLineFormatted(line, formatter, TextColor);
            }

            System.Console.SetCursorPosition(0, System.Console.CursorTop);
        }
    }
}
