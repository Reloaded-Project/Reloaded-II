using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NuGet.Protocol.Core.Types;
using Onova;
using Onova.Services;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;
using Reloaded.Mod.Shared;
using Reloaded.WPF.Utilities;
using MessageBox = System.Windows.MessageBox;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Contains static methods related to downloading loader, mods and updating them.
    /// </summary>
    public static class Update
    {
        /* Strings */
        private static XamlResource<string> _xamlCheckUpdatesFailed = new XamlResource<string>("ErrorCheckUpdatesFailed");

        /// <summary>
        /// Checks if there are any updates for the mod loader.
        /// </summary>
        public static async Task CheckForLoaderUpdatesAsync()
        {
            // Check for loader updates.
            try
            {
                using (var manager = new UpdateManager(
                    new GithubPackageResolver(SharedConstants.GitRepositoryAccount, SharedConstants.GitRepositoryName, SharedConstants.GitRepositoryReleaseName),
                    new ArchiveExtractor()))
                {
                    // Check for new version and, if available, perform full update and restart
                    var result = await manager.CheckForUpdatesAsync();
                    if (result.CanUpdate)
                    {
                        ActionWrappers.ExecuteWithApplicationDispatcher(() =>
                        {
                            var dialog = new ModLoaderUpdateDialog(manager, result.LastVersion);
                            dialog.ShowDialog();
                        });
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(_xamlCheckUpdatesFailed.Get());
            }
        }

        /// <summary>
        /// Checks if there are updates for any of the installed mods and/or new dependencies to fetch.
        /// </summary>
        public static async Task<bool> CheckForModUpdatesAsync()
        {
            var manageModsViewModel = IoC.Get<ManageModsViewModel>();
            var allMods = manageModsViewModel.Mods.Select(x => new PathGenericTuple<ModConfig>(x.ModConfigPath, (ModConfig) x.ModConfig)).ToArray();

            try
            {
                var updater       = new Updater(allMods, new UpdaterData(IoC.Get<AggregateNugetRepository>()));
                var updateDetails = await updater.GetUpdateDetails();

                if (updateDetails.HasUpdates())
                {
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        var dialog = new ModUpdateDialog(updater, updateDetails);
                        dialog.ShowDialog();
                        return true;
                    });
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message + "|" + e.StackTrace);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Downloads mods in an asynchronous fashion provided a list of Mod IDs to download.
        /// </summary>
        /// <param name="modIds">IDs of all of the mods to download.</param>
        /// <param name="includePrerelease">Include pre-release packages.</param>
        /// <param name="includeUnlisted">Include unlisted packages.</param>
        /// <param name="token">Used to cancel the operation.</param>
        public static async Task DownloadNuGetPackagesAsync(IEnumerable<string> modIds, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            var aggregateRepository = IoC.Get<AggregateNugetRepository>();
            var packages            = new List<NugetTuple<IPackageSearchMetadata>>();
            var missingPackages     = new List<string>();

            /* Get details of every mod. */
            foreach (var modId in modIds)
            {
                var packageDetails = await aggregateRepository.GetPackageDetails(modId, includePrerelease, includeUnlisted, token);
                var newest = aggregateRepository.GetNewestPackage(packageDetails);
                if (newest != null)
                    packages.Add(newest);
                else
                    missingPackages.Add(modId);
            }

            await DownloadNuGetPackagesAsync(packages, missingPackages, includePrerelease, includeUnlisted, token);
        }


        /// <summary>
        /// Downloads mods in an asynchronous fashion provided a list of known and missing packages.
        /// </summary>
        /// <param name="package">Existing known package.</param>
        /// <param name="missingPackages">List of packages known to be missing.</param>
        /// <param name="includePrerelease">Include pre-release packages.</param>
        /// <param name="includeUnlisted">Include unlisted packages.</param>
        /// <param name="token">Used to cancel the operation.</param>
        public static async Task DownloadNuGetPackagesAsync(NugetTuple<IPackageSearchMetadata> package, List<string> missingPackages, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            await DownloadNuGetPackagesAsync(new List<NugetTuple<IPackageSearchMetadata>>() { package }, missingPackages, includePrerelease, includeUnlisted, token);
        }

        /// <summary>
        /// Downloads mods in an asynchronous fashion provided a list of known and missing packages.
        /// </summary>
        /// <param name="packages">List of existing known packages.</param>
        /// <param name="missingPackages">List of packages known to be missing.</param>
        /// <param name="includePrerelease">Include pre-release packages.</param>
        /// <param name="includeUnlisted">Include unlisted packages.</param>
        /// <param name="token">Used to cancel the operation.</param>
        public static async Task DownloadNuGetPackagesAsync(List<NugetTuple<IPackageSearchMetadata>> packages, List<string> missingPackages, bool includePrerelease, bool includeUnlisted, CancellationToken token = default)
        {
            /* Get dependencies of every mod. */
            foreach (var package in packages.ToArray())
            {
                var repository = package.Repository;
                var searchResult = await repository.FindDependencies(package.Generic, includePrerelease, includeUnlisted, token);

                packages.AddRange(
                    searchResult.Dependencies.Select(x => new NugetTuple<IPackageSearchMetadata>(package.Repository, x)));
                missingPackages.AddRange(searchResult.PackagesNotFound);
            }

            /* Remove already existing packages. */
            var allMods = IoC.Get<ManageModsViewModel>().Mods.ToArray();
            HashSet<string> allModIds = new HashSet<string>(allMods.Length);
            foreach (var mod in allMods)
                allModIds.Add(mod.ModConfig.ModId);

            // Remove mods we already have.
            packages = packages.Where(x => !allModIds.Contains(x.Generic.Identity.Id)).ToList();
            missingPackages = missingPackages.Where(x => !allModIds.Contains(x)).ToList();

            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                var dialog = new NugetFetchPackageDialog(packages, missingPackages);
                dialog.ShowDialog();
            });
        }

        /// <summary>
        /// Verifies if all mods have all of their required dependencies and
        /// returns a list of missing dependencies by ModId.
        /// </summary>
        /// <returns>True if there ar missing dependencies, else false.</returns>
        public static bool CheckMissingDependencies(out List<string> missingDependencies)
        {
            var manageModsViewModel = IoC.Get<ManageModsViewModel>();

            // Get all mods and build list of IDs
            var allMods             = manageModsViewModel.Mods.ToArray();
            HashSet<string> allModIds = new HashSet<string>(allMods.Length);
            foreach (var mod in allMods)
                allModIds.Add(mod.ModConfig.ModId);

            // Build list of missing dependencies.
            var missingDeps = new HashSet<string>(allModIds.Count);
            foreach (var mod in allMods)
            {
                foreach (var dependency in mod.ModConfig.ModDependencies)
                {
                    if (! allModIds.Contains(dependency))
                        missingDeps.Add(dependency);
                }
            }

            missingDependencies = missingDeps.ToList();
            return missingDependencies.Count > 0;
        }
    }
}
