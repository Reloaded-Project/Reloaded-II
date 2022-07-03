
namespace Reloaded.Mod.Launcher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ReloadedWindow
{
    public Lib.Models.ViewModel.WindowViewModel RealViewModel { get; set; }

    public MainWindow()
    {
        // Make viewmodel of this window available.
        RealViewModel = IoC.GetConstant<Lib.Models.ViewModel.WindowViewModel>();

        // Initialize XAML.
        InitializeComponent();

        // Bind other models.
        IoC.Kernel.Bind<WPF.Theme.Default.WindowViewModel>().ToConstant((WPF.Theme.Default.WindowViewModel)DataContext); // Controls window properties.
        IoC.Kernel.Bind<MainWindow>().ToConstant(this);
    }
}