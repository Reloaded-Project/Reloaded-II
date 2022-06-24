using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages;

/// <summary>
/// The main page of the application.
/// </summary>
public partial class BasePage : ReloadedIIPage
{
    public MainPageViewModel ViewModel { get; set; }

    private CollectionViewSource _appsViewSource;

    public BasePage() : base()
    {
        InitializeComponent();
        ViewModel = IoC.Get<MainPageViewModel>();
        var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
        _appsViewSource = manipulator.Get<CollectionViewSource>("FilteredApps");

        this.AnimateInFinished += OnAnimateInFinished;
        this.KeyDown += TrySwitchPage;
    }

    /* Preconfigured Buttons */
    private void AddApp_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddApplicationCommand.Execute(null);
    }

    private void ManageMods_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Page = Page.ManageMods;
    }

    private void LoaderSettings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Page = Page.SettingsPage;
    }

    private void DownloadMods_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Page = Page.DownloadMods;
    }

    private void Application_Click(object sender, RoutedEventArgs e)
    {
        // Prepare for parameter transfer.
        if (sender is not FrameworkElement element) 
            return;

        if (element.DataContext is not PathTuple<ApplicationConfig> tuple) 
            return;

        ViewModel.SwitchToApplication(tuple);
    }

    private void TrySwitchPage(object sender, KeyEventArgs e)
    {
        if (!KeyboardUtils.TryGetPageScrollDirection(e, out int direction))
            return;
        
        ViewModel.SwitchPage(direction, _appsViewSource.View.Cast<PathTuple<ApplicationConfig>>().ToArray());
    }
}