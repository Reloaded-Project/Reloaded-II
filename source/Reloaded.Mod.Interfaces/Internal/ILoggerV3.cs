namespace Reloaded.Mod.Interfaces.Internal;

public interface ILoggerV3 : ILoggerV2
{
    /// <summary>
    /// Writes a new line to the console output
    /// </summary>
    /// <param name="message">The message to print.</param>
    /// <param name="color">The color to print the message in.</param>
    void WriteLineAsync(string message, Color color) { throw new NotImplementedException(); }

    /// <summary>
    /// Writes a new line to the console output
    /// </summary>
    /// <param name="message">The message to print.</param>
    void WriteLineAsync(string message) { throw new NotImplementedException(); }

    /// <summary>
    /// Writes additional text to the console output without proceeding to the next line.
    /// </summary>
    /// <param name="message">The message to print.</param>
    /// <param name="color">The color to print the message in.</param>
    void WriteAsync(string message, Color color) { throw new NotImplementedException(); }

    /// <summary>
    /// Writes additional text to the console output without proceeding to the next line.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void WriteAsync(string message) { throw new NotImplementedException(); }

    /// <summary>
    /// Blocks until console initialization has completed.
    /// </summary>
    void WaitForConsoleInit(CancellationToken token) { throw new NotImplementedException(); }
}