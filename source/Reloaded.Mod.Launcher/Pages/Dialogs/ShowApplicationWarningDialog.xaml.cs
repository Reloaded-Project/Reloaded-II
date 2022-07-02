using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for ShowApplicationWarningDialog.xaml
/// </summary>
public partial class ShowApplicationWarningDialog : ReloadedWindow
{
    public new AddApplicationWarningDialogViewModel ViewModel { get; set; }

    public ShowApplicationWarningDialog(AddApplicationWarningDialogViewModel viewmodel)
    {
        InitializeComponent();
        ViewModel = viewmodel;
    }
    
    private void OK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        this.Close();
    }
}