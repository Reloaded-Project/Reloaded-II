namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Dialogs;

/// <summary>
/// Interaction logic for ModSelectDialog.xaml
/// </summary>
public partial class LoadModSelectDialog : ReloadedWindow
{
    public new LoadModSelectDialogViewModel ViewModel { get; set; }

    private readonly CollectionViewSource _modsViewSource;

    /// <inheritdoc />
    public LoadModSelectDialog(LoadModSelectDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        // Setup filters
        var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
        _modsViewSource = manipulator.Get<CollectionViewSource>("FilteredMods");
        _modsViewSource.Filter += ModsViewSourceOnFilter;
    }

    /* Filtering Code */
    private void ModsViewSourceOnFilter(object sender, FilterEventArgs e)
    {
        if (this.ModsFilter.Text.Length <= 0)
        {
            e.Accepted = true;
            return;
        }

        var tuple = (PathTuple<ModConfig>) e.Item;
        e.Accepted = tuple.Config.ModName.Contains(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);
        if (! e.Accepted)
            e.Accepted = tuple.Config.ModId.Contains(this.ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);
    }

    private void ModsFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        _modsViewSource.View.Refresh();
    }

    /* Select/Pick Code */
    private void LoadMod_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadMod();
        this.Close();
    }
}