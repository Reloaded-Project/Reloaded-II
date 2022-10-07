using Reloaded.Mod.Launcher.Pages.Dialogs.EditModPackPages;
using Reloaded.Mod.Loader.Update.Packs;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

public partial class EditModPackDialog : ReloadedWindow
{
    public new EditModPackDialogViewModel ViewModel { get; set; }

    public EditModPackDialog(EditModPackDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private void OnClickMainPackageDetails(object sender, MouseButtonEventArgs e)
    {
        PageHost.CurrentPage = new EditMainPackDetailsPage(new EditMainPackDetailsPageViewModel(ViewModel.Pack));
    }

    private void OnClickIndividualModPage(object sender, MouseButtonEventArgs e)
    {
        var senderElement = (FrameworkElement)sender;
        var item = (ObservablePackItem)senderElement.DataContext;
        OpenModPage(item);
    }

    private void OpenModPage(ObservablePackItem item)
    {
        ViewModel.SelectedItem = item;
        var vm = new EditModPackDetailsPageViewModel(item);
        PageHost.CurrentPage = new EditModPackDetailsPage(vm);
    }

    private void RemoveMod(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.SelectedItem != null)
        {
            ViewModel.Pack.Items.Remove(ViewModel.SelectedItem);
            PageHost.CurrentPage = new EditMainPackDetailsPage(new EditMainPackDetailsPageViewModel(ViewModel.Pack));
        }
    }

    private async void AddMod(object sender, MouseButtonEventArgs e)
    {
        var dialogVm = new SelectPackModDialogViewModel(ViewModel.GetEligibleModsForSelection());
        var dialog = new SelectPackModDialog(dialogVm);
        if (!dialog.ShowDialog().GetValueOrDefault(false)) 
            return;

        var selectedMod = dialog.ViewModel.SelectedMod;
        if (selectedMod == null)
            return;

        // Actually add the selected mod and navigate page.
        OpenModPage(await ViewModel.AddModAsync(selectedMod)); // required by MagicScaler
    }

    private void LoadExistingPack(object sender, RoutedEventArgs e) => ViewModel.LoadExistingPack();

    private void SavePack(object sender, RoutedEventArgs e) => Task.Run(() => ViewModel.SavePack());

    private async void PreviewPack(object sender, RoutedEventArgs e)
    {
        var pack = await Task.Run(() => ViewModel.BuildPack(BmpImageConverter.Instance));
        var reader = new ReloadedPackReader(pack);
        var dialog = new InstallModPackDialog(new InstallModPackDialogViewModel(reader, Lib.IoC.Get<LoaderConfig>(), Lib.IoC.Get<AggregateNugetRepository>()));
        dialog.ShowDialog();
    }
}