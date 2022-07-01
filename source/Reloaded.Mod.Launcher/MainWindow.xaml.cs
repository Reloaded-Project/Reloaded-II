using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.WPF.Theme.Default;

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
        IoC.Kernel.Bind<WindowViewModel>().ToConstant((WindowViewModel)this.DataContext); // Controls window properties.
        IoC.Kernel.Bind<MainWindow>().ToConstant(this);
    }
}