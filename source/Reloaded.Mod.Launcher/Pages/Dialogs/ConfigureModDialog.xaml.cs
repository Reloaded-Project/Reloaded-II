using System.Windows;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for ConfigureModDialog.xaml
/// </summary>
public partial class ConfigureModDialog : ReloadedWindow
{
    public new ConfigureModDialogViewModel ViewModel { get; set; }

    public ConfigureModDialog(ConfigureModDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e) => ViewModel.Save();

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => ViewModel.Save();

    private void PropertyGridMakeExpandable(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemEventArgs e)
    {
        if (e.Item is PropertyItem item)
            if ((item.PropertyType.IsValueType && !item.PropertyType.IsEnum && !item.PropertyType.IsPrimitive) || item.PropertyType.IsClass)
                item.IsExpandable = true;
    }
}