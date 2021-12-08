using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using Reloaded.Mod.Launcher.Lib.Models.Model.DownloadPackagePage;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Shows the dialog for fetching new NuGet packages.
/// </summary>
public class NugetFetchPackageDialogViewModel : ObservableObject
{
    /// <summary>
    /// The packages to be fetched.
    /// </summary>
    public List<NugetTuple<IPackageSearchMetadata>> Packages { get; set; }
    
    /// <summary>
    /// List of packages that cannot be found.
    /// </summary>
    public List<string> MissingPackages { get; set; }

    /// <summary>
    /// True if missing packages part of the dialog should be shown, else false.
    /// </summary>
    public bool ShowMissingPackages { get; set; }

    /// <summary>
    /// Current status of the mod downloading operation.
    /// </summary>
    public DownloadPackageStatus DownloadPackageStatus { get; set; } = DownloadPackageStatus.Default;

    /// <summary>
    /// Whether the button/toggle/option to download the NuGet packages is enabled.
    /// </summary>
    public bool DownloadEnabled { get; set; }

    /// <summary/>
    public NugetFetchPackageDialogViewModel(List<NugetTuple<IPackageSearchMetadata>> packages, List<string> missingPackages)
    {
        Packages = packages;
        MissingPackages = missingPackages;
        DownloadEnabled = true;
        ShowMissingPackages = missingPackages.Count > 0;
    }

    /// <summary>
    /// Downloads and extracts all packages handled by this ViewModel.
    /// </summary>
    /// <returns></returns>
    public async Task DownloadAndExtractPackagesAsync()
    {
        DownloadEnabled = false;
        DownloadPackageStatus = DownloadPackageStatus.Downloading;

        var loaderConfig = IoC.Get<LoaderConfig>();
        foreach (var package in Packages)
        {
            var downloadPackage = await package.Repository.DownloadPackageAsync(package.Generic, CancellationToken.None);
            Nuget.ExtractPackage(downloadPackage, Path.Combine(loaderConfig.ModConfigDirectory, package.Generic.Identity.Id));
        }

        DownloadPackageStatus = DownloadPackageStatus.Default;
        DownloadEnabled = false;
    }
}