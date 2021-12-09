using System.Windows.Controls;
using System.Windows.Data;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Dependencies.xaml
/// </summary>
public partial class Dependencies : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    private readonly CollectionViewSource _dependenciesViewSource;

    public Dependencies(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        _dependenciesViewSource = new DictionaryResourceManipulator(this.Grid.Resources).Get<CollectionViewSource>("SortedDependencies");
        _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
    }

    private void ModsFilter_TextChanged(object sender, TextChangedEventArgs e) => _dependenciesViewSource.View.Refresh();

    private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e) => e.Accepted = ViewModel.FilterMod((BooleanGenericTuple<IModConfig>)e.Item);
}