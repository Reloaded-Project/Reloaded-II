using Reloaded.Mod.Launcher.Commands.EditAppPage;
using Reloaded.Mod.Launcher.Models.ViewModel.Dialogs.FirstLaunch;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.FirstLaunchPages;

/// <summary>
/// Interaction logic for AddApplication.xaml
/// </summary>
public partial class AddApplication : ReloadedPage
{
    public AddApplicationViewModel ViewModel { get; set; } = IoC.Get<AddApplicationViewModel>();

    public AddApplication()
    {
        InitializeComponent();
    }

    private void SkipTutorial_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.FirstLaunchViewModel.Close();

    private void SkipStep_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ViewModel.FirstLaunchViewModel.GoToNextStep();
    }

    private void AddApplication_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var parameters = new AddApplicationCommand.AddApplicationCommandParams();
        ViewModel.AddApplicationCommand.Execute(parameters);

        if (parameters.ResultCreatedApplication)
            ViewModel.FirstLaunchViewModel.GoToNextStep();
    }
}