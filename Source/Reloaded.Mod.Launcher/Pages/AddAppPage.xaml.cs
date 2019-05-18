using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using WindowViewModel = Reloaded.Mod.Launcher.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class AddAppPage : ReloadedIIPage
    {
        private SplashViewModel _addGameViewModel;

        public AddAppPage() : base()
        {  
            InitializeComponent();
        }
    }
}
