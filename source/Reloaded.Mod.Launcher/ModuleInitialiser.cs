namespace Reloaded.Mod.Launcher;

internal class ModuleInitialiser
{
    [ModuleInitializer]
    public static void Init()
    {
        // Raise maximum number of WebRequest connections
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
    }
}