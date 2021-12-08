using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

/// <summary>
/// Interaction logic for CreateModDialog.xaml
/// </summary>
public partial class EditModDialog : ReloadedWindow
{
    public EditModDialogViewModel RealViewModel  { get; set; }

    private readonly CollectionViewSource _dependenciesViewSource;

    public EditModDialog(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        RealViewModel = viewModel;

        this.Closing += OnClosing;
        _dependenciesViewSource = new DictionaryResourceManipulator(this.Contents.Resources).Get<CollectionViewSource>("SortedDependencies");
        _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
    }

    private void OnClosing(object sender, CancelEventArgs e) => RealViewModel.Save();

    private void ModIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;

        RealViewModel.SetNewImage();
    }

    private void Save_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;
            
        Close();
    }

    private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e) => e.Accepted = RealViewModel.FilterItem((BooleanGenericTuple<IModConfig>) e.Item);

    /* Check if not duplicate. */

    private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _dependenciesViewSource.View.Refresh();
}