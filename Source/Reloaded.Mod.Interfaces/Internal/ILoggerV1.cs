using System;
using System.Drawing;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface ILoggerV1
    {
        // Events
        
        /// <summary>
        /// Executed after printing a line to the console.
        /// </summary>
        event EventHandler<string> OnPrintMessage;

        // Print Message

        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="color">The colour to print the message in.</param>
        void PrintMessage(string message, Color color);

        // Console Colours
        Color BackgroundColor { get; set; }
        Color TextColor { get; set; } 

        Color ColorRed { get; set; } 
        Color ColorRedLight { get; set; } 
        Color ColorGreen { get; set; } 
        Color ColorGreenLight { get; set; } 
        Color ColorYellow { get; set; } 
        Color ColorYellowLight { get; set; } 
        Color ColorBlue { get; set; }
        Color ColorBlueLight { get; set; }
        Color ColorPink { get; set; } 
        Color ColorPinkLight { get; set; }
        Color ColorLightBlue { get; set; }
        Color ColorLightBlueLight { get; set; }
    }
}
