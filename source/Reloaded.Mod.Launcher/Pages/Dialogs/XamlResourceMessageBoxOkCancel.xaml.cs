namespace Reloaded.Mod.Launcher.Pages.Dialogs;

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
        this.CancelBtn.Content = Lib.Static.Resources.ResourceProvider.Get<string>(cancelButtonTextResourceName);
        this.OKBtn.Content = Lib.Static.Resources.ResourceProvider.Get<string>(okButtonTextResourceName);
    }
}