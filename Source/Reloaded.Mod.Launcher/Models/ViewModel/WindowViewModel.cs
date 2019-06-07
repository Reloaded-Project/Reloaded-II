using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class WindowViewModel : ObservableObject
    {
        /// <summary>
        /// The currently displayed page on this window.
        /// </summary>
        public Pages.Page CurrentPage
        {
            get;
            set;
        } = Pages.Page.Splash;

        /// <summary>
        /// The title of the main window.
        /// </summary>
        public string WindowTitle
        {
            get;
            set;
        } = "Reloaded II";
    }
}
