using System;
using System.Collections.Generic;
using System.IO;

namespace Reloaded.Mod.Loader.Logging
{
    /// <summary>
    /// Basic logger used for storing crash logs
    /// </summary>
    public class Logger : IDisposable
    {
        private const int MaxLogLength = 1000;

        /// <summary>
        /// Gets the path to which the log will be flushed to.
        /// </summary>
        public string FlushPath { get; }

        private Console _console;
        private Queue<string> _log;

        /// <summary>
        /// Intercepts logs from the console and provides the ability to flush them to a file in the event of a crash.
        /// </summary>
        /// <param name="console">The console to intercept logs from.</param>
        /// <param name="outputDir">The directory to which the log is output to.</param>
        public Logger(Console console, string outputDir)
        {
            _log = new Queue<string>(MaxLogLength);
            _console = console;
            _console.OnPrintMessage += OnPrintMessge;
            FlushPath = Path.Combine(outputDir, "LoaderCrashLog.txt");
        }

        public void Flush() => File.WriteAllLines(FlushPath, _log);
        private void OnPrintMessge(object sender, string message)
        {
            if (_log.Count == MaxLogLength)
                _log.Dequeue();

            _log.Enqueue(message);
        }

        /// <inheritdoc />
        public void Dispose() => _console.OnPrintMessage -= OnPrintMessge;
    }
}
