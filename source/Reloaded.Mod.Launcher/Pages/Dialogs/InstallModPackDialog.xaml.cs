using Reloaded.Mod.Launcher.Pages.Dialogs.InstallModPackPages;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for InstallModPackDialog.xaml
/// </summary>
public partial class InstallModPackDialog : ReloadedWindow
{
    public new InstallModPackDialogViewModel ViewModel { get; }

    public InstallModPackDialog(InstallModPackDialogViewModel viewModel)
    {
        // To register the codec.
        _ = JxlImageConverter.Instance;
        ViewModel = viewModel;
        InitializeComponent();
        InitInitialPage();
        ViewModel.PropertyChanged += OnPageIndexPropertyChanged;
        this.Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (PageHost.CurrentPage is IDisposable disposable)
            disposable.Dispose();
    }

    private void OnPageIndexPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.PageIndex))
            return;

        var index = ViewModel.PageIndex;
        if (index == 0)
        {
            InitInitialPage();
            return;
        }

        var itemOffset = index - 1;
        if (itemOffset < ViewModel.Mods.Count)
        {
            InitForMod(itemOffset);
            return;
        }

        // Init download page.
        PageHost.CurrentPage = new InstallModDownloadPage(ViewModel);
    }

    private void InitForMod(int itemOffset)
    {
        PageHost.CurrentPage = new InstallModPackModPage(new InstallModPackModPageViewModel(ViewModel, ViewModel.Mods[itemOffset]));
    }

    private void InitInitialPage()
    {
        PageHost.CurrentPage = new InstallModPackEntryPage(new InstallModPackEntryPageViewModel(ViewModel));
    }
}