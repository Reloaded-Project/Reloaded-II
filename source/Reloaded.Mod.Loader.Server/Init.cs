namespace Reloaded.Mod.Loader.Server;

internal class Init
{
    [ModuleInitializer]
    public static void Initialise()
    {
        // TODO: FEC is broken on .NET 7, will potentially be for a while.
        TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.Compile();
    }
}