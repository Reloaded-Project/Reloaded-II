#pragma warning disable CS1591
namespace Reloaded.Mod.Launcher.Lib.Misc;

public static class Constants
{
    public const string DialogJsonFilter = "Json (*.json)|*.json";
    public const string DialogSupportedFormatsFilter = "(*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg; *.jpeg; *.jpe; *.jfif; *.png";

    public const string ParameterKill = "--kill";
    public const string ParameterLaunch = "--launch";
    public const string ParameterArguments = "--arguments";
    public const string ParameterWorkingDirectory = "--working-directory";
    public const string ParameterDownload  = "--download";
    public const string ParameterR2Pack = "--r2pack";
    public const string ParameterR2PackDownload = "--r2packdl";

    public const string GitRepositoryAccount = "Reloaded-Project";
    public const string GitRepositoryName = "Reloaded-II";
    public const string GitRepositoryReleaseName = "Release.zip";

    public const string ReloadedProtocol = "R2";
    public const string ReloadedPackProtocol = "R2Pack";

    public static readonly string ApplicationPath      = Process.GetCurrentProcess().MainModule!.FileName!;
    public static readonly string ApplicationDirectory = Path.GetDirectoryName(ApplicationPath)!;

    public const string VersionFileName = "version.txt";
    public static readonly string VersionFilePath = Path.Combine(ApplicationDirectory, VersionFileName);
}