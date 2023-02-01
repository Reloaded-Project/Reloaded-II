using static Reloaded.Mod.Installer.NativeImports;

namespace Reloaded.Mod.Installer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        // Hide the console window if not doing a commandline task.
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
    }
}