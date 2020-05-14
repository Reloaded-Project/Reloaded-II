using System;
using System.IO;
using System.Linq;
using PeNet;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Properties;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Utility
{
    public class AsiLoaderDeployer
    {
        public ImageApplicationPathTuple Application { get; }

        /// <summary>
        /// Deploys Ultimate ASI Loader to a given application profile.
        /// </summary>
        public AsiLoaderDeployer(ImageApplicationPathTuple application)
        {
            Application = application;
        }

        /// <summary>
        /// True if executable is 64bit, else false.
        /// </summary>
        /// <param name="filePath">Path of the EXE file.</param>
        public bool Is64Bit(string filePath) => new PeFile(filePath).Is64Bit;

        /// <summary>
        /// Checks if the ASI loader can be deployed.
        /// </summary>
        public bool CanDeploy()
        {
            if (!File.Exists(Application.Config.AppLocation))
                return false;

            var peHeader     = new PeFile(Application.Config.AppLocation);
            using var stream = new FileStream(Application.Config.AppLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
            return GetFirstDllFile(peHeader, stream) != null;
        }

        /// <summary>
        /// Deploys the ASI loader to the game folder.
        /// </summary>
        public void DeployAsiLoader(out string loaderPath, out string bootstrapperPath)
        {
            loaderPath = null;
            DeployBootstrapper(out bool loaderAlreadyInstalled, out bootstrapperPath);
            if (!loaderAlreadyInstalled)
            {
                var appDirectory = Path.GetDirectoryName(Application.Config.AppLocation);
                var peHeader     = new PeFile(Application.Config.AppLocation);
                using var stream = new FileStream(Application.Config.AppLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
                string dllName   = GetFirstDllFile(peHeader, stream);
                var loaderDll    = peHeader.Is64Bit ? Resources.ASILoader64 : Resources.ASILoader32;
                loaderPath       = Path.Combine(appDirectory, dllName);
                File.WriteAllBytes(loaderPath, loaderDll);
            }
        }

        /// <summary>
        /// Gets the location to install the bootstrapper to.
        /// </summary>
        private string GetBootstrapperInstallPath(out bool alreadyInstalled)
        {
            alreadyInstalled = false;

            if (IsLoaderAlreadyInstalled(out string installPath))
            {
                alreadyInstalled = true;
                return installPath;
            }

            var appDirectory = Path.GetDirectoryName(Application.Config.AppLocation);
            var pluginDirectory = Path.Combine(appDirectory, AsiCommonDirectories[0]);
            Directory.CreateDirectory(pluginDirectory);
            return pluginDirectory;
        }

        private void DeployBootstrapper(out bool loaderAlreadyInstalled, out string bootstrapperInstallPath)
        {
            bootstrapperInstallPath = "";

            var installFolder    = GetBootstrapperInstallPath(out loaderAlreadyInstalled);
            var bootstrapperPath = Is64Bit(Application.Config.AppLocation) ? IoC.Get<LoaderConfig>().Bootstrapper64Path
                                                                           : IoC.Get<LoaderConfig>().Bootstrapper32Path;
            string[] filesToCopy = Directory.GetFiles(Path.GetDirectoryName(bootstrapperPath), "*.dll", SearchOption.TopDirectoryOnly);
            string fileToRename  = Path.GetFileName(bootstrapperPath);

            foreach (var file in filesToCopy)
            {
                var fileName = Path.GetFileName(file);
                if (fileName != fileToRename)
                    File.Copy(file, Path.Combine(installFolder, fileName), true);
                else
                {
                    bootstrapperInstallPath = Path.Combine(installFolder, Path.ChangeExtension(fileName, PluginExtension));
                    File.Copy(file, bootstrapperInstallPath, true);
                }
            }
        }

        /// <summary>
        /// Returns true if loader is already installed, else false.
        /// </summary>
        public bool IsLoaderAlreadyInstalled(out string modPath)
        {
            var appDirectory = Path.GetDirectoryName(Application.Config.AppLocation);
            foreach (var directory in AsiCommonDirectories)
            {
                var directoryPath = Path.Combine(appDirectory, directory);
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
        /// Get name of DLL file using which ASI loader can be installed.
        /// </summary>
        /// <param name="peFile">The PE file to look for first DLL in.</param>
        /// <param name="headerStream">Stream with the current pointer set to the start of the PE file.</param>
        private string GetFirstDllFile(PeFile peFile, Stream headerStream)
        {
            string GetSupportedDll(PeFile file, Stream stream, string[] supportedDlls)
            {
                var names = file.ImageImportDescriptors.GetNames(file, stream);
                return names.FirstOrDefault(x => supportedDlls.Contains(x, StringComparer.OrdinalIgnoreCase));
            }

            return GetSupportedDll(peFile, headerStream, peFile.Is32Bit ? AsiLoaderSupportedDll32 : AsiLoaderSupportedDll64);
        }

        #region Constants
        private static string PluginExtension = ".asi";

        private static readonly string[] AsiLoaderSupportedDll32 = new[]
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

        private static readonly string[] AsiLoaderSupportedDll64 = new[]
        {
            "wininet.dll",
            "version.dll",
            "dsound.dll",
            "dinput8.dll"
        };

        private static readonly string[] AsiCommonDirectories = new[]
        {
            "scripts",
            "plugins"
        };
        #endregion
    }
}
