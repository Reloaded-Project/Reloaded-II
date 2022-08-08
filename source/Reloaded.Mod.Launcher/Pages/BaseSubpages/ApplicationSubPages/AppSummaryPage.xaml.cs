using Reloaded.WPF.Controls;
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
        SwappedOut += Dispose;
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
    {
        if (ModsFilter.Text.Length <= 0)
        {
            e.Accepted = true;
            return;
        }

        var tuple = (ModEntry)e.Item;
        e.Accepted = tuple.Tuple.Config.ModName.Contains(ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);
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