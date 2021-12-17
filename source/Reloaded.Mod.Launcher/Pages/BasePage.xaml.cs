using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Pages;

/// <summary>
/// The main page of the application.
/// </summary>
public partial class BasePage : ReloadedIIPage
{
    public MainPageViewModel ViewModel { get; set; }

    public BasePage() : base()
    {
        InitializeComponent();
        ViewModel = IoC.Get<MainPageViewModel>();
        this.AnimateInFinished += OnAnimateInFinished;
    }

    /* Preconfigured Buttons */
    private void AddApp_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ViewModel.AddApplicationCommand.Execute(null);
    }

    private void ManageMods_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ViewModel.Page = Page.ManageMods;
    }

    private void LoaderSettings_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ViewModel.Page = Page.SettingsPage;
    }

    private void DownloadMods_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        ViewModel.Page = Page.DownloadMods;
    }

    private void Application_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Prepare for parameter transfer.
        if (sender is not FrameworkElement element) 
            return;

        if (element.DataContext is not PathTuple<ApplicationConfig> tuple) 
            return;

        ViewModel.SwitchToApplication(tuple);
    }
}