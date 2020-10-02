using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Colorful;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Logging.Init;
using Reloaded.Mod.Loader.Logging.Structs;

namespace Reloaded.Mod.Loader.Logging
{
    public class Console : ILogger
    {
        public event EventHandler<string> OnPrintMessage = (sender, s) => { };

        /// <summary>
        /// Indicates if console is ready to be used or not.
        /// If console is not yet ready, all write operations will be buffered into the async collection.
        /// </summary>
        public bool ConsoleReady { get; private set; }

        private BlockingCollection<LogMessage> _messages = new BlockingCollection<LogMessage>();
        private Thread _loggingThread;
        private bool _consoleAllocated = false;
        private bool _initializationStarted = false;

        /* Default constructor */
        public Console() { }

        /// <summary>
        /// Asynchronously initializes the console.
        /// </summary>
        /// <param name="isEnabled">True to enable the console, else false.</param>
        public void InitConsoleAsync(bool isEnabled)
        {
            if (_initializationStarted) 
                return;

            _initializationStarted = true;
            if (!isEnabled)
                return;

            Task.Run(() =>
            {
                _consoleAllocated = ConsoleAllocator.Alloc();
                Colorful.Console.BackgroundColor = BackgroundColor;
                Colorful.Console.ForegroundColor = TextColor;
                Colorful.Console.Clear();

                _loggingThread = new Thread(ProcessQueue) { IsBackground = true };
                _loggingThread.Start();

                PrintBanner();
                ConsoleReady = true;
            });
        }

        public void PrintMessage(string message, Color color)   => WriteLine(message, color);
        public void WriteLine(string message)                   => WriteLine(message, TextColor);
        public void Write(string message)                       => Write(message, TextColor);

        public void WriteLine(string message, Color color)
        {
            if (!_consoleAllocated)
                return;

            if (ConsoleReady)
            {
                Colorful.Console.WriteLine(message, color);
                OnPrintMessage?.Invoke(this, message);
            }
            else
            {
                WriteLineAsync(message, color);
            }
        }

        public void Write(string message, Color color)
        {
            if (!_consoleAllocated)
                return;

            if (ConsoleReady)
            {
                Colorful.Console.Write(message, color);
                OnPrintMessage?.Invoke(this, message);
            }
            else
            {
                WriteAsync(message, color);
            }
        }

        public void WriteLineAsync(string message) => WriteLineAsync(message, TextColor);
        public void WriteLineAsync(string message, Color color) => _messages.Add(new LogMessage(LogMessageType.WriteLine, message, color));

        public void WriteAsync(string message) => WriteAsync(message, TextColor);
        public void WriteAsync(string message, Color color) => _messages.Add(new LogMessage(LogMessageType.Write, message, color));

        // Default Colours
        public Color BackgroundColor     { get; set; } = Color.FromArgb(20, 25, 31);
        public Color TextColor           { get; set; } = Color.FromArgb(239, 240, 235);

        public Color ColorRed            { get; set; } = Color.FromArgb(255, 92, 87);
        public Color ColorRedLight       { get; set; } = Color.FromArgb(220, 163, 163);

        public Color ColorGreen          { get; set; } = Color.FromArgb(90, 247, 142);
        public Color ColorGreenLight     { get; set; } = Color.FromArgb(195, 191, 159);

        public Color ColorYellow         { get; set; } = Color.FromArgb(243, 249, 157);
        public Color ColorYellowLight    { get; set; } = Color.FromArgb(240, 223, 175);

        public Color ColorBlue           { get; set; } = Color.FromArgb(87, 199, 255);
        public Color ColorBlueLight      { get; set; } = Color.FromArgb(148, 191, 243);

        public Color ColorPink           { get; set; } = Color.FromArgb(255, 106, 193);
        public Color ColorPinkLight      { get; set; } = Color.FromArgb(236, 147, 211);

        public Color ColorLightBlue      { get; set; } = Color.FromArgb(154, 237, 254);
        public Color ColorLightBlueLight { get; set; } = Color.FromArgb(147, 224, 227);

        /// <summary>
        /// Synchronously waits for console initialization using blocking.
        /// </summary>
        public void WaitForConsoleInit(CancellationToken token = default)
        {
            if (!_initializationStarted)
                return;

            while (!ConsoleReady)
                Thread.Sleep(1);
        }

        private void ProcessQueue()
        {
            while (true)
            {
                WaitForConsoleInit();
                var message = _messages.Take();

                switch (message.Type)
                {
                    default:
                    case LogMessageType.WriteLine:
                        WriteLine(message.Message, message.Color);
                        break;
                    case LogMessageType.Write:
                        Write(message.Message, message.Color);
                        break;
                }
            }
        }

        /* Default Banner */
        #region Banner: Reloaded Branding
        public void PrintBanner()
        {
            System.Console.Write("\n\n");

            Formatter[] formatter =
            {
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

            string[] template =
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


            WriteLinesCentered(template, formatter);
            System.Console.Write("\n");
            PrintCoreVersion();
            System.Console.Write("\n\n");
        }

        private void PrintCoreVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            var coreVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            Formatter[] formatters =
            {
                new Formatter($"{version}", TextColor),
                new Formatter($"{coreVersion}", ColorRed)
            };

            WriteLineCentered("{0} // {1}", formatters);
        }

        private void WriteLinesCentered(string[] lines, Formatter[] formatters)
        {
            for (int x = 0; x < lines.Length; x++)
            {
                var line = lines[x];
                var formatter = formatters[x];

                WriteLineCentered(line, formatter);
            }
        }

        private void WriteLineCentered(string line, params Formatter[] formatters)
        {
            var formattedLine = String.Format(line, formatters.Select(x => x.Target).ToArray());

            CenterCursor(formattedLine);
            Colorful.Console.WriteLineFormatted(line, TextColor, formatters);
            System.Console.SetCursorPosition(0, System.Console.CursorTop);
        }

        private void CenterCursor(string finalText)
        {
            // Get center, accounting for overflow.
            int consolePointer = (System.Console.WindowWidth - finalText.Length) / 2;
            if (consolePointer < 0)
                consolePointer = 0;

            System.Console.SetCursorPosition(consolePointer, System.Console.CursorTop);
        }
        #endregion
    }
}
