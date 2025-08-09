namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for downloading an individual package.
/// </summary>
public class InstallPackageViewModel : INotifyPropertyChanged
{
    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
    }

    private double _progress;
    public double Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }
    }

    private bool _isComplete;
    public bool IsComplete
    {
        get => _isComplete;
        set
        {
            if (_isComplete != value)
            {
                _isComplete = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsComplete)));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}