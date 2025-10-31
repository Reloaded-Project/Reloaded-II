namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for downloading an individual package.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class InstallPackageViewModel : INotifyPropertyChanged
{
    public string Text { get; set; }
    public string Title { get; set; }
    public double Progress { get; set; }
    public bool IsComplete { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}