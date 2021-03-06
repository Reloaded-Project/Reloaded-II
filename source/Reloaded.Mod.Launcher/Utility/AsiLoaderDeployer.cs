﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility.Parsers;
using SharpCompress.Archives.SevenZip;

namespace Reloaded.Mod.Launcher.Utility
{
    public class AsiLoaderDeployer
    {
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
            if (!File.Exists(Application.Config.AppLocation))
                return false;

            using var peParser = new BasicPeParser(Application.Config.AppLocation);

            try
            {
                return GetFirstDllFile(peParser) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deploys the ASI loader to the game folder.
        /// </summary>
        public void DeployAsiLoader(out string loaderPath, out string bootstrapperPath)
        {
            loaderPath = null;
            DeployBootstrapper(out bool loaderAlreadyInstalled, out bootstrapperPath);
            if (loaderAlreadyInstalled) 
                return;

            using var peParser = new BasicPeParser(Application.Config.AppLocation);
            var appDirectory = Path.GetDirectoryName(Application.Config.AppLocation);
            var dllName      = GetFirstDllFile(peParser);
            loaderPath       = Path.Combine(appDirectory, dllName);
            ExtractAsiLoader(loaderPath, !peParser.Is32BitHeader);
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
        /// Get name of first DLL file using which ASI loader can be installed.
        /// </summary>
        /// <param name="peParser">Parsed PE file.</param>
        private string GetFirstDllFile(BasicPeParser peParser)
        {
            string GetSupportedDll(BasicPeParser file, string[] supportedDlls)
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
            var libraryDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var compressedLoaderPath = $"{libraryDirectory}/Loader/Asi/UltimateAsiLoader.7z";
            var archive = SevenZipArchive.Open(compressedLoaderPath);
            ExtractFileWithName(is64bit ? "ASILoader64.dll" : "ASILoader32.dll");

            void ExtractFileWithName(string name)
            {
                foreach (var entry in archive.Entries)
                {
                    if (!String.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var stream = entry.OpenEntryStream();
                    using var writeStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write);
                    stream.CopyTo(writeStream);
                    break;
                }
            }
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
            "wininet.dll",
            "version.dll",
            "dsound.dll",
            "dinput8.dll"
        };

        private static readonly string[] AsiCommonDirectories = 
        {
            "scripts",
            "plugins"
        };
        #endregion
    }
}
