namespace Reloaded.Mod.Loader.Logging;

/// <summary>
/// Proxy for System.Console. Used as a fallback on Wine and other potentially problematic terminals.
/// </summary>
public class SystemConsoleProxy : IConsoleProxy
{
    /// <inheritdoc />
    public void WriteLine(string text) => System.Console.WriteLine(text);

    /// <inheritdoc />
    public void Write(string text) => System.Console.Write(text);

    /// <inheritdoc />
    public void WriteLine(string text, Color color)
    {
        var currentColor = System.Console.ForegroundColor;
        SetForeColor(color);
        WriteLine(text);
        System.Console.ForegroundColor = currentColor;
    }

    /// <inheritdoc />
    public void Write(string text, Color color)
    {
        var currentColor = System.Console.ForegroundColor;
        SetForeColor(color);
        Write(text);
        System.Console.ForegroundColor = currentColor;
    }

    /// <inheritdoc />
    public void Clear() => System.Console.Clear();

    /// <inheritdoc />
    public void SetForeColor(Color color) => System.Console.ForegroundColor = color.ToNearestConsoleColor();

    /// <inheritdoc />
    public void SetBackColor(Color color) => System.Console.BackgroundColor = color.ToNearestConsoleColor();

    /// <inheritdoc />
    public void SetCursorPosition(int left, int top) => System.Console.SetCursorPosition(left, top);
}