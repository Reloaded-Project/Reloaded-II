using System;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReloadedWindow
    {
        public Models.ViewModel.WindowViewModel RealViewModel { get; set; }

        public MainWindow()
        {
            // Make viewmodel of this window available.
            RealViewModel = IoC.GetConstant<Models.ViewModel.WindowViewModel>();

            // Initialize XAML.
            InitializeComponent();

            // Bind other models.
            IoC.Kernel.Bind<WindowViewModel>().ToConstant((WindowViewModel)this.DataContext); // Controls window properties.
            IoC.Kernel.Bind<MainWindow>().ToConstant(this);
            this.Closed += (sender, args) => { Environment.Exit(0); };
        }
    }
}
