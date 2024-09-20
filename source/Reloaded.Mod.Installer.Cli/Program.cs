using System;
using System.IO;
using System.Threading.Tasks;
using Reloaded.Mod.Installer.DependencyInstaller.IO;
using ProgressBar = ConsoleProgressBar.ProgressBar;

namespace Reloaded.Mod.Installer.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Reloaded-II CLI Installer\n" +
                          "Mode flags:\n" +
                          "--dependenciesOnly: Don't install Reloaded, just install runtimes.");
        
        if (!Cli.TryRunCli(args))
            Cli.InstallInCli();
    }
}