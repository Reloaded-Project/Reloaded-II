using System;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Static;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// Interaction logic for DownloadModsPage.xaml
/// </summary>
public partial class DownloadPackagesPage : ReloadedIIPage
{
    public DownloadPackagesViewModel ViewModel { get; set; }

    public DownloadPackagesPage()
    {
        InitializeComponent();

        ViewModel = IoC.Get<DownloadPackagesViewModel>();
        this.AnimateInStarted += TryGetSearchResults;
    }

    private async void TryGetSearchResults()
    {
        try
        {
            await ViewModel.GetSearchResults();
        }
        catch (Exception ex)
        {
            Errors.HandleException(ex, "Failed to search for mods to download.");
        }
    }
}