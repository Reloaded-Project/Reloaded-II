using System.Windows.Documents;
using System.Windows.Navigation;
using WindowViewModel = Reloaded.Mod.Launcher.Lib.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages;

/// <summary>
/// Special type of <see cref="ReloadedWindow"/> that adds localization support
/// to the existing windows as well as auto-changes Window titles.
/// </summary>
// ReSharper disable once InconsistentNaming
public class ReloadedIIPage : ReloadedPage, INotifyPropertyChanged
{
    public ReloadedIIPage()
    {
        this.AnimateInFinished += OnAnimateInFinished;
        this.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(Page_RequestNavigate));
    }

    protected virtual void OnAnimateInFinished()
    {
        // Change window title to current page title.
        if (! String.IsNullOrEmpty(this.Title))
            Lib.IoC.Get<WindowViewModel>().WindowTitle = this.Title;
    }

    // For automatic implementation via Fody.
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Page_RequestNavigate(object sender, RequestNavigateEventArgs e) => e.Handled = true;
}