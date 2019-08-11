using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        }
    }
}
