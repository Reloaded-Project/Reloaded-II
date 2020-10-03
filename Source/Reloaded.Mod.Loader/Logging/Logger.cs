using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Reloaded.Mod.Loader.Logging
{
    /// <summary>
    /// Basic logger used for storing crash logs
    /// </summary>
    public class Logger : IDisposable
    {
        private const int MaxBufferLength = 1000;

        /// <summary>
        /// Gets the path to which the log will be flushed to.
        /// </summary>
        public string FlushPath { get; }

        private StreamWriter  _textStream;
        private Console       _console;
        private List<string>  _logItems;

        /// <summary>
        /// Intercepts logs from the console and provides the ability to flush them to a file in the event of a crash.
        /// </summary>
        /// <param name="console">The console to intercept logs from.</param>
        /// <param name="outputDir">The directory to which the log is output to.</param>
        public Logger(Console console, string outputDir)
        {
            var executableName    = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            var universalDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss");
            Directory.CreateDirectory(outputDir);

            _logItems = new List<string>(MaxBufferLength + 1);
            _console  = console;
            _console.OnPrintMessage += OnPrintMessage;

            FlushPath   = Path.Combine(outputDir, $"{universalDateTime} ~ {executableName}.txt");
            _textStream = File.CreateText(FlushPath);
            _textStream.AutoFlush = false;
        }

        /// <summary>
        /// Flushes the current contents of the log.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Flush()
        {
            foreach (var item in _logItems) 
                _textStream.WriteLine(item);

            _textStream.Flush();
            _logItems.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnPrintMessage(object sender, string message)
        {
            if (_logItems.Count == MaxBufferLength)
                Flush();

            _logItems.Add(message);
        }

        /// <inheritdoc />
        public void Dispose() => _console.OnPrintMessage -= OnPrintMessage;
    }
}
