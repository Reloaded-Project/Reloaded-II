namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Various utilities related to UI Navigation.
/// </summary>
internal class NavigationUtils
{
    /// <summary>
    /// Normalizes a given direction to -1, 0, 1 range.
    /// </summary>
    internal static int NormalizeDirection(int direction)
    {
        switch (direction)
        {
            case 0:
                return direction;
            case > 0:
                return 1;
            case < 0:
                return -1;
        }
    }
}