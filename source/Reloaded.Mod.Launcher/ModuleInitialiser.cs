namespace Reloaded.Mod.Launcher;

internal class ModuleInitialiser
{
    [ModuleInitializer]
    public static void Init()
    {
        // Raise maximum number of WebRequest connections
        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        
        // When .NET makes first HTTP request, it takes some time to resolve proxy settings.
        // We can speed this up by resolving the proxy ourselves.
        Task.Run(WebRequest.GetSystemWebProxy);
    }
}