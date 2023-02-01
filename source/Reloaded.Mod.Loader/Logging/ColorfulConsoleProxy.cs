namespace Reloaded.Mod.Loader.Logging;

/// <summary>
/// Proxy for Colorful.Console. [Default]
/// </summary>
public class ColorfulConsoleProxy : IConsoleProxy
{
    /// <inheritdoc />
    public void WriteLine(string text) => Colorful.Console.WriteLine(text);

    /// <inheritdoc />
    public void Write(string text) => Colorful.Console.Write(text);

    /// <inheritdoc />
    public void WriteLine(string text, Color color) => Colorful.Console.WriteLine(text, color);

    /// <inheritdoc />
    public void Write(string text, Color color) => Colorful.Console.Write(text, color);

    /// <inheritdoc />
    public void Clear() => Colorful.Console.Clear();

    /// <inheritdoc />
    public void SetForeColor(Color color) => Colorful.Console.ForegroundColor = color;

    /// <inheritdoc />
    public void SetBackColor(Color color) => Colorful.Console.BackgroundColor = color;

    /// <inheritdoc />
    public void SetCursorPosition(int left, int top) => Colorful.Console.SetCursorPosition(left, top);
}