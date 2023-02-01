namespace Reloaded.Mod.Loader.Logging.Interfaces;

public interface IConsoleProxy
{
    void WriteLine(string text);
    void Write(string text);
    void WriteLine(string text, Color color);
    void Write(string text, Color color);
    void Clear();
    void SetForeColor(Color color);
    void SetBackColor(Color color);
    void SetCursorPosition(int left, int top);
}