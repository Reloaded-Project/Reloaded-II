namespace Reloaded.Mod.Launcher.Utility;

public static class ThemeHelpers
{

    public static void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
    {
        ProcessExtensions.OpenHyperlink(e.Parameter.ToString()!);
        e.Handled = true;
    }
}
