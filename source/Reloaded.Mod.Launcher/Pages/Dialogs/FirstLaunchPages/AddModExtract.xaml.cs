using System.Windows.Navigation;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.FirstLaunchPages;

/// <summary>
/// Interaction logic for AddModExtract.xaml
/// </summary>
public partial class AddModExtract : VideoTutorialPage
{
    public AddModExtract()
    {
        InitializeComponent();
    }

    private void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        => ProcessExtensions.OpenFileWithDefaultProgram(e.Uri.ToString());
}