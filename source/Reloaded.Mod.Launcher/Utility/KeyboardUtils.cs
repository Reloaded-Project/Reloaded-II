using System.Windows.Input;

namespace Reloaded.Mod.Launcher.Utility;

internal static class KeyboardUtils
{
    internal const Key Accept = Key.Space;
    internal const Key Modifier = Key.LeftCtrl;

    internal const Key Up = Key.Up;
    internal const Key Down = Key.Down;

    /// <summary>
    /// Gets the list/array scroll direction based on pressed keyboard button.
    /// </summary>
    internal static bool TryGetListScrollDirection(KeyEventArgs e, out int direction)
    {
        direction = e.Key switch
        {
            Up => -1,
            Down => 1,
            _ => 0
        };

        return direction != 0;
    }
}