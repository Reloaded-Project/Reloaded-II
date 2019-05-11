using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReloadedWindow
    {
        /// <summary>
        /// The currently displayed page on this window.
        /// </summary>
        public Pages.Page CurrentPage { get; set; } = Pages.Page.Base;

        public MainWindow()
        {
            InitializeComponent();

            // Make viewmodel of this window available.
            IoC.Kernel.Bind<WindowViewModel>().ToConstant((WindowViewModel)this.DataContext);
        }
    }
}
