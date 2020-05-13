using System;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for DownloadModsPage.xaml
    /// </summary>
    public partial class DownloadModsPage : ReloadedIIPage, IDisposable
    {
        public DownloadModsViewModel ViewModel { get; set; }

        public DownloadModsPage()
        {
            InitializeComponent();
            ViewModel = IoC.GetConstant<DownloadModsViewModel>();
            this.AnimateOutStarted += Dispose;
        }

        public void Dispose()
        {
            ViewModel?.Dispose();
        }
    }
}
