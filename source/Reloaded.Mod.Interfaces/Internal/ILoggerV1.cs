namespace Reloaded.Mod.Interfaces.Internal;

public interface ILoggerV1
{
    // Events
        
    /// <summary>
    /// [Legacy] Executed when a user of the logger calls <see cref="ILoggerV2.WriteLine(string)"/>, <see cref="ILoggerV2.Write(string)"/> and derivatives.
    /// </summary>
    [Obsolete("Use OnWriteLine instead. This event exists for backwards compatibility with original API only.")]
    event EventHandler<string> OnPrintMessage;

    // Print Message

    /// <summary>
    /// This is an alias for <see cref="ILoggerV2.WriteLine(string,System.Drawing.Color)"/>
    /// </summary>
    /// <param name="message">The message to print.</param>
    /// <param name="color">The color to print the message in.</param>
    void PrintMessage(string message, Color color) { throw new NotImplementedException(); }

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