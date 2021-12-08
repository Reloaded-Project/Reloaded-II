using System;
using System.Windows.Controls;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs.FirstLaunchPages;

public class VideoTutorialPage : ReloadedPage
{
    protected void OnMediaEnded(object sender, System.Windows.RoutedEventArgs e)
    {
        var player = (MediaElement)e.Source;
        player.Position = TimeSpan.Zero;
    }

    protected void Next_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        IoC.Get<FirstLaunchViewModel>().GoToNextStep();
    }

    protected void Previous_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        IoC.Get<FirstLaunchViewModel>().GoToLastStep();
    }
}