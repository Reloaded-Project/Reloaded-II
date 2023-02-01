namespace Reloaded.Mod.Interfaces.Internal;

public interface ILoggerV4 : ILoggerV3
{
    /// <summary>
    /// Executed when a user of the logger calls <see cref="ILoggerV2.WriteLine(string)"/> and its overloads.
    /// </summary>
    event EventHandler<(string text, Color color)> OnWriteLine;

    /// <summary>
    /// Executed when a user of the logger calls <see cref="ILoggerV2.Write(string)"/> and its overloads.
    /// </summary>
    event EventHandler<(string text, Color color)> OnWrite;
}