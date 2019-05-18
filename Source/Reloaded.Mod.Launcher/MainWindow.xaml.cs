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
            InitializeComponent();
            RealViewModel = new Models.ViewModel.WindowViewModel();

            // Make viewmodel of this window available.
            IoC.Kernel.Bind<WindowViewModel>().ToConstant((WindowViewModel)this.DataContext);
            IoC.Kernel.Bind<MainWindow>().ToConstant(this);
            IoC.Kernel.Bind<Models.ViewModel.WindowViewModel>().ToConstant(RealViewModel);
        }
    }
}
