using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Shared;
using Reloaded.WPF.Pages.Animations;
using Reloaded.WPF.Pages.Animations.Enum;
using ApplicationSubPage = Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages.Enum.ApplicationSubPage;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : ReloadedIIPage, IDisposable
    {
        public ApplicationViewModel ViewModel { get; set; }

        public ApplicationPage()
        {
            InitializeComponent();
            ViewModel = new ApplicationViewModel(IoC.Get<MainPageViewModel>().SelectedApplication, IoC.Get<ManageModsViewModel>());
            this.AnimateOutStarted += AnimateOutChildAndDispose;
        }

        ~ApplicationPage()
        {
            Dispose();
        }

        public void Dispose()
        {
            ViewModel?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void AnimateOutChildAndDispose()
        {
            PageHost.CurrentPage?.AnimateOut();
            Dispose();
        }

        private void ReloadedMod_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is FrameworkElement element)
                {
                    if (element.DataContext is Process process)
                    {
                        ViewModel.SelectedProcess = process;
                        ViewModel.ChangePageProperty(ApplicationSubPage.ReloadedProcess);
                    }
                }
            }
        }

        private void NonReloadedMod_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is FrameworkElement element)
                {
                    if (element.DataContext is Process process)
                    {
                        ViewModel.SelectedProcess = process;
                        ViewModel.ChangePageProperty(ApplicationSubPage.NonReloadedProcess);
                    }
                }
            }
        }

        private void Summary_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ViewModel.ChangePageProperty(ApplicationSubPage.ApplicationSummary);
            }
        }

        private void LaunchApplication_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var appConfig = ViewModel.ApplicationTuple.ApplicationConfig;
                var launcher = ApplicationLauncher.FromApplicationConfig(appConfig);
                launcher.Start();
            }
        }

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
                IoC.Get<WindowViewModel>().WindowTitle = $"{this.Title}: {ViewModel.ApplicationTuple.ApplicationConfig.AppName}";
        }
    }
}
