using System.Windows;
using System.Windows.Input;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageBoxOkCancel.xaml
    /// </summary>
    public class XamlResourceMessageBoxOkCancel : MessageBoxOkCancel
    {
        public XamlResourceMessageBoxOkCancel(string titleResourceName, string descriptionResourceName) : base(new XamlResource<string>(titleResourceName).Get(), new XamlResource<string>(descriptionResourceName).Get())
        {

        }

        public XamlResourceMessageBoxOkCancel(string titleResourceName, string descriptionResourceName, string okButtonTextResourceName, string cancelButtonTextResourceName) : base(new XamlResource<string>(titleResourceName).Get(), new XamlResource<string>(descriptionResourceName).Get())
        {
            this.CancelBtn.Content = new XamlResource<string>(cancelButtonTextResourceName).Get();
            this.OKBtn.Content = new XamlResource<string>(okButtonTextResourceName).Get();
        }
    }
}
