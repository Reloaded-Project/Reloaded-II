using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Utility class that simplifies the deployment of Ultimate ASI Loader to a given application.
/// </summary>
public class AsiLoaderDeployer
{
    /// <summary>
    /// The application for which the ASI Loader will be deployed.
    /// </summary>
    public PathTuple<ApplicationConfig> Application { get; }

    /// <summary>
    /// Deploys Ultimate ASI Loader to a given application profile.
    /// </summary>
    public AsiLoaderDeployer(PathTuple<ApplicationConfig> application)
    {
        Application = application;
    }

    /// <summary>
    /// True if executable is 64bit, else false.
    /// </summary>
    /// <param name="filePath">Path of the EXE file.</param>
    public bool Is64Bit(string filePath)
    {
        using var parser = new BasicPeParser(filePath);
        return !parser.Is32BitHeader;
    }

    /// <summary>
    /// Checks if the ASI loader can be deployed.
    /// </summary>
    public bool CanDeploy()
    {
        var appLocation = ApplicationConfig.GetAbsoluteAppLocation(Application);
        if (!File.Exists(appLocation))
            return false;

        try
        {
            using var peParser = new BasicPeParser(appLocation);
            return GetSupportedDllFromParser(peParser);
        }
        catch (Exception e)
        {
            Errors.HandleException(e, Resources.ErrorCantReadExeFileAsiLoaderDeploy.Get());
            return false;
        }
    }

    private bool GetSupportedDllFromParser(BasicPeParser peParser)
    {
        try
        {
            return GetFirstSupportedDllFile(peParser) != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Deploys the ASI loader (if needed) and bootstrapper to the game folder.
    /// </summary>
    public void DeployAsiLoader(out string? asiLoaderPath, out string bootstrapperPath)
    {
        asiLoaderPath = null;
        DeployBootstrapper(out bool alreadyHasAsiPlugins, out bootstrapperPath);
        if (alreadyHasAsiPlugins) 
            return;

        var appLocation = ApplicationConfig.GetAbsoluteAppLocation(Application);
        using var peParser = new BasicPeParser(appLocation);
        var appDirectory = Path.GetDirectoryName(appLocation);
        var dllName      = GetFirstSupportedDllFile(peParser);
        asiLoaderPath    = Path.Combine(appDirectory!, dllName!);
        ExtractAsiLoader(asiLoaderPath, !peParser.Is32BitHeader);
    }

    /// <summary>
    /// Gets the path to which the bootstrapper will be copied to should it be installed.
    /// </summary>
    /// <param name="alreadyHasAsiPlugins">True if at least 1 ASI plugin is already installed.</param>
    /// <returns>The path to which the bootstrapper will be copied to.</returns>
    public string GetBootstrapperInstallPath(out bool alreadyHasAsiPlugins)
    {
        var installFolder = GetBootstrapperInstallFolder(out alreadyHasAsiPlugins);
        var bootstrapperPath = GetBootstrapperDllPath();
        return Path.Combine(installFolder, Path.ChangeExtension(Path.GetFileName(bootstrapperPath), PluginExtension));
    }

    /// <summary>
    /// Gets the path of the bootstrapper DLL to copy.
    /// </summary>
    public string GetBootstrapperDllPath()
    {
        return Is64Bit(ApplicationConfig.GetAbsoluteAppLocation(Application))
            ? IoC.Get<LoaderConfig>().Bootstrapper64Path
            : IoC.Get<LoaderConfig>().Bootstrapper32Path;
    }

    /// <summary>
    /// Returns true if ASI loader is already installed, else false.
    /// This check works by checking the existence of ASI files in a supported directory.
    /// </summary>
    private bool AreAnyAsiPluginsInstalled(out string? modPath)
    {
        var appDirectory = Path.GetDirectoryName(ApplicationConfig.GetAbsoluteAppLocation(Application));
        foreach (var directory in AsiCommonDirectories)
        {
            var directoryPath = Path.Combine(appDirectory!, directory);

            if (!Directory.Exists(directoryPath))
                continue;

            if (!Directory.GetFiles(directoryPath).Any(x => x.EndsWith(PluginExtension, StringComparison.OrdinalIgnoreCase)))
                continue;

            modPath = directoryPath;
            return true;
        }

        modPath = null;
        return false;
    }

    /// <summary>
    /// Gets the folder to install the bootstrapper to.
    /// Returned folder should be created if it did not previously exist.
    /// </summary>
    /// <param name="alreadyHasAsiPlugins">Whether a supported folder with at least one ASI Plugin already exists. Assume loader already installed if it does.</param>
    private string GetBootstrapperInstallFolder(out bool alreadyHasAsiPlugins)
    {
        alreadyHasAsiPlugins = false;

        if (AreAnyAsiPluginsInstalled(out string? installPath))
        {
            alreadyHasAsiPlugins = true;
            return installPath!;
        }

        var appDirectory    = Path.GetDirectoryName(ApplicationConfig.GetAbsoluteAppLocation(Application));
        var pluginDirectory = Path.Combine(appDirectory!, AsiCommonDirectories[0]);
        return pluginDirectory;
    }

    private void DeployBootstrapper(out bool alreadyHasAsiPlugins, out string bootstrapperInstallPath)
    {
        bootstrapperInstallPath = GetBootstrapperInstallPath(out alreadyHasAsiPlugins);
        var bootstrapperDir = Path.GetDirectoryName(bootstrapperInstallPath);

        if (!Directory.Exists(bootstrapperDir))
        {
            try
            {
                Directory.CreateDirectory(bootstrapperDir!);
            }
            catch (Exception e)
            {
                throw new IOException(string.Format(Resources.BootstrapperCreateDirectoryError.Get(), e.Message));
            }
        }

        File.Copy(GetBootstrapperDllPath(), bootstrapperInstallPath, true);
    }

    /// <summary>
    /// Get name of first DLL file using which ASI loader can be installed.
    /// </summary>
    /// <param name="peParser">Parsed PE file.</param>
    private string? GetFirstSupportedDllFile(BasicPeParser peParser)
    {
        string? GetSupportedDll(BasicPeParser file, string[] supportedDlls)
        {
            var names = file.GetImportDescriptorNames();
            return names.FirstOrDefault(x => supportedDlls.Contains(x, StringComparer.OrdinalIgnoreCase));
        }

        return GetSupportedDll(peParser, peParser.Is32BitHeader ? AsiLoaderSupportedDll32 : AsiLoaderSupportedDll64);
    }

    /// <summary>
    /// Extracts the ASI loader to a given path.
    /// </summary>
    /// <param name="filePath">Absolute file path to extract loader to.</param>
    /// <param name="is64bit">Whether loader is 64 bit or not.</param>
    private void ExtractAsiLoader(string filePath, bool is64bit)
    {
        var libraryDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        var compressedLoaderPath = $"{libraryDirectory}/Loader/Asi/UltimateAsiLoader.7z";
            
        var archive = new SevenZipExtractor(compressedLoaderPath);
        using var writeStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write);
        archive.ExtractFile(is64bit ? "ASILoader64.dll" : "ASILoader32.dll", writeStream);
    }

    #region Constants
    private static string PluginExtension = ".asi";

    private static readonly string[] AsiLoaderSupportedDll32 = 
    {
        "xlive.dll",
        "winmm.dll",
        "wininet.dll",
        "vorbisFile.dll",
        "version.dll",
        "msvfw32.dll",
        "msacm32.dll",
        "dsound.dll",
        "dinput8.dll",
        "dinput.dll",
        "ddraw.dll",
        "d3d11.dll",
        "d3d9.dll",
        "d3d8.dll"
    };

    private static readonly string[] AsiLoaderSupportedDll64 = 
    {
        "winmm.dll",
        "wininet.dll",
        "version.dll",
        "dsound.dll",
        "dinput8.dll"
    };

    private static readonly string[] AsiCommonDirectories = 
    {
        "", // root folder
        "scripts",
        "plugins"
    };
    #endregion
}