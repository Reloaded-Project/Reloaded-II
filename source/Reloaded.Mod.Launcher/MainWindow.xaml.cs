
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
        RealViewModel = Lib.IoC.GetConstant<Lib.Models.ViewModel.WindowViewModel>();

        // Initialize XAML.
        InitializeComponent();

        // Bind other models.
        Lib.IoC.BindToConstant((WPF.Theme.Default.WindowViewModel)DataContext);// Controls window properties.
        Lib.IoC.BindToConstant(this);
    }
}