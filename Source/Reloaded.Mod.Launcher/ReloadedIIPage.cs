using System;
using Reloaded.WPF.Theme.Default;
using WindowViewModel = Reloaded.Mod.Launcher.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Special type of <see cref="ReloadedWindow"/> that adds localization support
    /// to the existing windows as well as auto-changes Window titles.
    /// </summary>
    public class ReloadedIIPage : ReloadedPage
    {
        public ReloadedIIPage()
        {
            this.AnimateInFinished += OnAnimateInFinished;
        }

        protected virtual void OnAnimateInFinished()
        {
            // Change window title to current page title.
            if (! String.IsNullOrEmpty(this.Title))
                IoC.Get<WindowViewModel>().WindowTitle = this.Title;
        }
    }
}
