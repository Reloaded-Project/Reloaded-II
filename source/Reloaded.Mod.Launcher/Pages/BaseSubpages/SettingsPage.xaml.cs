using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Commands.General;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : ReloadedIIPage
{
    public SettingsPageViewModel ViewModel { get; set; }

    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = IoC.GetConstant<SettingsPageViewModel>();
        this.AnimateOutStarted += OnLeavingPage;
        IoC.Get<MainWindow>().Closing += OnMainWindowExit;
    }

    private void OnMainWindowExit(object sender, CancelEventArgs e) => OnLeavingPage();
    private async void OnLeavingPage() => await ViewModel.SaveConfigAsync();

    private void Documents_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => new OpenDocumentationCommand().Execute(null);
    private async void LanguageChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => await ViewModel.SaveNewLanguageAsync();
    private async void ThemeChanged(object sender, SelectionChangedEventArgs e) => await ViewModel.SaveNewThemeAsync();

    private void Discord_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram("https://discord.gg/A8zNnS6");
    private void Twitter_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram("https://twitter.com/TheSewer56");
    private void Donate_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram("https://github.com/sponsors/Sewer56");

    private void LogFiles_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ViewModel.OpenLogFileLocation();
    private void ConfigFile_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ViewModel.OpenConfigFile();
}