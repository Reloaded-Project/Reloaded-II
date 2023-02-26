using System.Linq;
using Button = Sewer56.UI.Controller.Core.Enums.Button;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;

/// <summary>
/// Interaction logic for ApplicationSummaryPage.xaml
/// </summary>
public partial class AppSummaryPage : ApplicationSubPage, IDisposable
{
    public ConfigureModsViewModel ViewModel { get; set; }
    private readonly DictionaryResourceManipulator _manipulator;
    private readonly CollectionViewSource _modsViewSource;
    private bool _disposed;

    public AppSummaryPage(ApplicationViewModel appViewModel)
    {
        InitializeComponent();
        ViewModel = new ConfigureModsViewModel(appViewModel, Lib.IoC.Get<ModUserConfigService>());
        
        ControllerSupport.SubscribeCustomInputs(OnProcessCustomInputs);
        _manipulator    = new DictionaryResourceManipulator(this.Contents.Resources);
        _modsViewSource = _manipulator.Get<CollectionViewSource>("FilteredMods");
        _modsViewSource.Filter += ModsViewSourceOnFilter;
        ViewModel.PropertyChanged += OnFilterChanged;
        SwappedOut += Dispose;
    }

    private void OnFilterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedTag))
            _modsViewSource.View.Refresh();
    }

    ~AppSummaryPage() => Dispose();

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ControllerSupport.UnsubscribeCustomInputs(OnProcessCustomInputs);
        ViewModel?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ModsViewSourceOnFilter(object sender, FilterEventArgs e)
    {;
        var tuple = (ModEntry)e.Item;
        e.Accepted = true;

        // Filter name
        var config = tuple.Tuple.Config;
        if (ModsFilter.Text.Length > 0)
            e.Accepted = config.ModName.Contains(ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);

        if (e.Accepted == false)
            return;

        // Filter tag
        if (ViewModel.SelectedTag != ConfigureModsViewModel.IncludeAllTag)
        {
            e.Accepted = config.Tags.Contains(ViewModel.SelectedTag);

            if (e.Accepted != false)
                return;

            // Try auto tags
            bool hasCodeInjection = config.HasDllPath();
            if (hasCodeInjection && ViewModel.SelectedTag == ConfigureModsViewModel.CodeInjectionTag)
            {
                e.Accepted = true;
                return;
            }
            else if (!hasCodeInjection && ViewModel.SelectedTag == ConfigureModsViewModel.NoCodeInjectionTag)
            {
                e.Accepted = true;
                return;
            }

            // Auto tag: Universal mod
            if (ViewModel.SelectedTag == ConfigureModsViewModel.NoUniversalModsTag)
                e.Accepted = !config.IsUniversalMod;

            if (ViewModel.SelectedTag == ConfigureModsViewModel.NativeModTag)
                e.Accepted = config.IsNativeMod("");
        }
    }

    private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _modsViewSource.View.Refresh();
    }

    #region Keyboard Controls
    private void KeyboardControls_KeyDown(object sender, KeyEventArgs e)
    {
        // Toggle On/Off
        if (e.Key == KeyboardUtils.Accept)
            ToggleModItem();

        ProcessKeyboardItemShift(sender, e);
    }

    private static void ProcessKeyboardItemShift(object sender, KeyEventArgs e)
    {
        // Shift item up/down
        if (!KeyboardUtils.TryGetListScrollDirection(e, out int indexOffset))
            return;

        if (!TryShiftSelectedItem((ListView)sender, indexOffset))
            return;

        // Needed so our selection doesn't skip
        e.Handled = true;
    }
    #endregion

    #region Controller Controls

    private void OnProcessCustomInputs(in ControllerState state, ref bool handled)
    {
        if (!WpfUtilities.TryGetFocusedElementAndWindow(out var window, out var focused))
            return;

        // We only deal with the listview.
        if (focused is not ListViewItem item)
            return;
        
        if (state.IsButtonPressed(Button.Accept))
            ToggleModItem();

        ProcessControllerItemShift(state, item);
    }

    private static void ProcessControllerItemShift(in ControllerState state, ListViewItem listViewItem)
    {
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
        if (!state.IsButtonHeld(Button.Modifier))
            return;

        if (!ControllerSupport.TryGetListScrollDirection(state, out int indexOffset))
            return;

        var listView = WpfUtilities.FindParent<ListView>(listViewItem!);
        if (listView == null)
            return;

        TryShiftSelectedItem(listView, indexOffset);
    }
    #endregion

    private static bool TryShiftSelectedItem(ListView listView, int indexOffset)
    {
        if (!listView.ShiftItem(indexOffset))
            return false;

        KeyboardNav.Focus((UIElement)listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex));
        return true;
    }

    private void ToggleModItem()
    {
        var mod = ViewModel.SelectedMod;
        if (mod == null || mod.Enabled == null)
            return;

        mod.Enabled = !mod.Enabled;
    }
}