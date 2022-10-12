using Window = System.Windows.Window;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// The main page of the application.
/// </summary>
public partial class ManageModsPage : ReloadedIIPage, IDisposable
{
    public ManageModsViewModel ViewModel { get; set; }
    private readonly CollectionViewSource _modsViewSource;
    private readonly CollectionViewSource _appsViewSource;
    private bool _disposed;
    private MainWindow _mainWindow;

    public ManageModsPage() : base()
    {
        SwappedOut += Dispose;
        InitializeComponent();
        ViewModel = Lib.IoC.GetConstant<ManageModsViewModel>();
        this.DataContext = ViewModel;
        _mainWindow = Lib.IoC.GetConstant<MainWindow>();
        _mainWindow.Closing += OnMainWindowClosing;

        // Setup filters
        var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
        _modsViewSource = manipulator.Get<CollectionViewSource>("SortedMods");
        _appsViewSource = manipulator.Get<CollectionViewSource>("SortedApps");
        _modsViewSource.Filter += ModsViewSourceOnFilter;
        _appsViewSource.Filter += AppsViewSourceOnFilter;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        SaveCurrentMod();
        _modsViewSource.Filter -= ModsViewSourceOnFilter;
        _appsViewSource.Filter -= AppsViewSourceOnFilter;
        _mainWindow.Closing -= OnMainWindowClosing;
    }

    private void OnMainWindowClosing(object? sender, CancelEventArgs e) => Dispose();

    private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _modsViewSource.View.Refresh();
    private void AppsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _appsViewSource.View.Refresh();
    private void AppsViewSourceOnFilter(object sender, FilterEventArgs e) => FilterText(AppsFilter.Text, ((BooleanGenericTuple<ApplicationConfig>)e.Item).Generic.AppName, e);
    private void ModsViewSourceOnFilter(object sender, FilterEventArgs e) => FilterText(ModsFilter.Text, ((PathTuple<ModConfig>)e.Item).Config.ModName, e);

    private void FilterText(string textToFind, string textToCheck, FilterEventArgs e)
    {
        if (textToFind.Length <= 0)
        {
            e.Accepted = true;
            return;
        }

        e.Accepted = textToCheck.Contains(textToFind, StringComparison.InvariantCultureIgnoreCase);
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        var createModDialog = new CreateModDialog(ViewModel.ModConfigService);
        createModDialog.Owner = Window.GetWindow(this);
        createModDialog.ShowDialog();
    }

    private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Tell viewmodel to swap ModId compatibility chart.
        PathTuple<ModConfig>? oldModTuple = null!;
        PathTuple<ModConfig>? newModTuple = null!;
        if (e.RemovedItems.Count > 0)
            oldModTuple = e.RemovedItems[0] as PathTuple<ModConfig>;

        if (e.AddedItems.Count > 0)
            newModTuple = e.AddedItems[0] as PathTuple<ModConfig>;

        ViewModel.SetNewMod(oldModTuple, newModTuple);
        e.Handled = true;
    }

    private void SaveCurrentMod() => ViewModel.SaveMod(ViewModel.SelectedModTuple);

    private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        ViewModel.SetNewImage();
        e.Handled = true;
    }

}