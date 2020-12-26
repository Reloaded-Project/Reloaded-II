using System;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for DownloadModsPage.xaml
    /// </summary>
    public partial class DownloadModsPage : ReloadedIIPage
    {
        public DownloadModsViewModel ViewModel { get; set; }

        public DownloadModsPage()
        {
            InitializeComponent();

            ViewModel = IoC.GetConstant<DownloadModsViewModel>();
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
}
