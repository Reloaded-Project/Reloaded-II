namespace Reloaded.Mod.Launcher.Utility;

internal static class KeyboardUtils
{
    internal const Key Accept = Key.Space;
    internal const Key Modifier = Key.LeftCtrl;

    internal const Key ModNextPage = Key.E;
    internal const Key ModLastPage = Key.Q;

    internal const Key Up = Key.Up;
    internal const Key Down = Key.Down;

    /// <summary>
    /// Tries to get the page scroll direction based on input.
    /// </summary>
    internal static bool TryGetPageScrollDirection(KeyEventArgs e, out int direction)
    {
        direction = 0;
        if (!Keyboard.IsKeyDown(Modifier))
            return false;

        direction = e.Key switch
        {
            ModLastPage => -1,
            ModNextPage => 1,
            _ => 0
        };

        return direction != 0;
    }

    /// <summary>
    /// Gets the list/array scroll direction based on pressed keyboard button.
    /// </summary>
    internal static bool TryGetListScrollDirection(KeyEventArgs e, out int direction)
    {
        direction = 0;
        if (!Keyboard.IsKeyDown(Modifier))
            return false;

        direction = e.Key switch
        {
            Up => -1,
            Down => 1,
            _ => 0
        };

        return direction != 0;
    }
}