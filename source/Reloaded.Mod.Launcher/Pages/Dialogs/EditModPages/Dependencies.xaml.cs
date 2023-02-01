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