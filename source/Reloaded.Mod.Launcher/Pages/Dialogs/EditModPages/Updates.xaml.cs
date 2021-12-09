using System;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Updates.xaml
/// </summary>
public partial class Updates : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    public Updates(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }
}