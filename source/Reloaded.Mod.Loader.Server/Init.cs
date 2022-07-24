namespace Reloaded.Mod.Loader.Server;

internal class Init
{
    [ModuleInitializer]
    public static void Initialise()
    {
        TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
    }
}