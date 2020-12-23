using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Utility;

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
