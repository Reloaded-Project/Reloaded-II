namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for MessageBoxOkCancel.xaml
/// </summary>
public class XamlResourceMessageBoxOkCancel : MessageBoxOkCancel
{
    public XamlResourceMessageBoxOkCancel(string titleResourceName, string descriptionResourceName) : base(new XamlResource<string>(titleResourceName).Get(), new XamlResource<string>(descriptionResourceName).Get())
    {

    }

    public XamlResourceMessageBoxOkCancel(string title, string message, string okText, string cancelText) : base(title, message)
    {
        this.OKBtn.Content = okText;
        this.CancelBtn.Content = cancelText;
    }
}