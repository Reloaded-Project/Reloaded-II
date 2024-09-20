namespace Reloaded.Mod.Installer;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (Cli.Cli.TryRunCli(args))
            return;

        var application = new App();
        application.InitializeComponent();
        application.Run();
    }
}