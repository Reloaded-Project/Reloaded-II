using System;
using System.Drawing;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Logging.Init;

namespace Reloaded.Mod.Loader.Logging
{
    public class Console : ILogger
    {
        public event EventHandler<string> OnPrintMessage = (sender, s) => { };

        /* Default constructor */
        public Console()
        {
            ConsoleAllocator.Alloc();
        }

        public void PrintMessage(string message, Color color)
        {
            message = $"{GetCurrentTime()} {message}";
            Colorful.Console.WriteLine(message, color);
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

        // Implementation
        private string GetCurrentTime()
        {
            return $"[{DateTime.Now:hh:mm:ss}]";
        }
    }
}
