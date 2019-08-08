using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface ILoggerV2
    {
        /// <summary>
        /// Writes a new line to the console output
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="color">The color to print the message in.</param>
        void WriteLine(string message, Color color);

        /// <summary>
        /// Writes a new line to the console output
        /// </summary>
        /// <param name="message">The message to print.</param>
        void WriteLine(string message);

        /// <summary>
        /// Writes additional text to the console output without proceeding to the next line.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="color">The color to print the message in.</param>
        void Write(string message, Color color);

        /// <summary>
        /// Writes additional text to the console output without proceeding to the next line.
        /// </summary>
        /// <param name="message">The message to print.</param>
        void Write(string message);
    }
}
