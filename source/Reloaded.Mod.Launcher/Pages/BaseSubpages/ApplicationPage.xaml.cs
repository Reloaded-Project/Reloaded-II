using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Misc;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.WPF.Pages.Animations;
using Reloaded.WPF.Pages.Animations.Enum;
using ApplicationSubPage = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.ApplicationSubPage;
using Environment = Reloaded.Mod.Shared.Environment;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// Interaction logic for ApplicationPage.xaml
/// </summary>
public partial class ApplicationPage : ReloadedIIPage, IDisposable
{
    public ApplicationViewModel ViewModel { get; set; }

    public ApplicationPage()
    {
        InitializeComponent();
        ViewModel = new ApplicationViewModel(IoC.Get<MainPageViewModel>().SelectedApplication!, IoC.Get<ModConfigService>(), IoC.Get<LoaderConfig>());
        this.AnimateOutStarted += Dispose;
    }

    ~ApplicationPage()
    {
        Dispose();
    }

    public void Dispose()
    {
        this.AnimateOutStarted -= Dispose;
        ActionWrappers.ExecuteWithApplicationDispatcher(() => PageHost.CurrentPage?.AnimateOut());
        ViewModel?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ReloadedMod_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;

        if (sender is not FrameworkElement element) 
            return;

        if (element.DataContext is not Process process) 
            return;

        ViewModel.SelectedProcess = process;
        ViewModel.ChangeApplicationPage(ApplicationSubPage.ReloadedProcess);
    }

    private void NonReloadedMod_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;

        if (sender is not FrameworkElement element) 
            return;

        if (element.DataContext is not Process process) 
            return;

        ViewModel.SelectedProcess = process;
        if (!process.HasExited)
            ViewModel.ChangeApplicationPage(ApplicationSubPage.NonReloadedProcess);
    }

    private void Summary_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;

        ViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
    }

    private void Edit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;

        ViewModel.ChangeApplicationPage(ApplicationSubPage.EditApplication);
    }

    private async void LaunchApplication_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) 
            return;

        await ViewModel.ApplicationTuple.SaveAsync();
        ViewModel.EnforceModCompatibility();
        await Setup.CheckForMissingModDependenciesAsync();

        var appConfig = ViewModel.ApplicationTuple.Config;
        var launcher  = ApplicationLauncher.FromApplicationConfig(appConfig);

        if (!Environment.IsWine || (Environment.IsWine && CompatibilityDialogs.WineShowLaunchDialog()))
            launcher.Start();
    }


    private void MakeShortcut_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.MakeShortcutCommand.CanExecute(null))
            ViewModel.MakeShortcutCommand.Execute(null);
    }

    private void LoadModSet_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ViewModel.LoadModSet();
    private void SaveModSet_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ViewModel.SaveModSet();

    /* Animation/Title/Setup overrides */
    protected override Animation[] MakeExitAnimations()
    {
        return new Animation[]
        {
            new RenderTransformAnimation(-this.ActualWidth, RenderTransformDirection.Horizontal, RenderTransformTarget.Away, null, XamlExitSlideAnimationDuration.Get()),
            new OpacityAnimation(XamlExitFadeAnimationDuration.Get(), 1, XamlExitFadeOpacityEnd.Get())
        };
    }

    protected override void OnAnimateInFinished()
    {
        if (!String.IsNullOrEmpty(this.Title))
            IoC.Get<WindowViewModel>().WindowTitle = $"{this.Title}: {ViewModel.ApplicationTuple.Config.AppName}";
    }
}