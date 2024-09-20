namespace Reloaded.Mod.Installer.Lib.Utilities;

/// <summary>
/// An abstract class that implements the bare minimum of the INotifyPropertyChanged interface.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
}