namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Special.xaml
/// </summary>
public partial class Complete : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    private readonly CollectionViewSource _dependenciesViewSource;

    public Complete(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        _dependenciesViewSource = new DictionaryResourceManipulator(this.Grid.Resources).Get<CollectionViewSource>("SortedApplications");
        _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
    }

    private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e) => e.Accepted = ViewModel.FilterApp((BooleanGenericTuple<IApplicationConfig>)e.Item);

    private void AppsFilter_TextChanged(object sender, TextChangedEventArgs e) => _dependenciesViewSource.View.Refresh();
}