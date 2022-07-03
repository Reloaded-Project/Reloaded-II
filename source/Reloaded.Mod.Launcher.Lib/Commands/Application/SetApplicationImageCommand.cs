namespace Reloaded.Mod.Launcher.Lib.Commands.Application;

/// <summary>
/// Command which allows a person to set a new application image.
/// </summary>
public class SetApplicationImageCommand : ICommand
{
    private readonly PathTuple<ApplicationConfig>? _applicationConfig;

    /// <summary/>
    public SetApplicationImageCommand(PathTuple<ApplicationConfig>? applicationConfig)
    {
        _applicationConfig = applicationConfig;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _applicationConfig != null;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        // Select image
        string imagePath = SelectImageFile();

        if (String.IsNullOrEmpty(imagePath) || ! File.Exists(imagePath))
            return;

        // Get current selected application and its paths.
        string? applicationDirectory = Path.GetDirectoryName(_applicationConfig!.Path);

        // Get application entry in set of all applications.
        string applicationIconFileName = Path.GetFileName(imagePath);
        if (applicationDirectory != null)
        {
            string applicationIconPath = Path.Combine(applicationDirectory, applicationIconFileName);

            // Copy image and set config file path.
            File.Copy(imagePath, applicationIconPath, true);
            _applicationConfig.Config.AppIcon = applicationIconFileName;
            _applicationConfig.Save();
        }
    }

    /// <summary>
    /// Opens up a file selection dialog allowing for the selection of a custom image to associate with the profile.
    /// </summary>
    private string SelectImageFile()
    {
        var dialog    = new VistaOpenFileDialog();
        dialog.Title  = Resources.AddAppImageSelectorTitle.Get();
        dialog.Filter = $"{Resources.AddAppImageSelectorFilter.Get()} {Constants.DialogSupportedFormatsFilter}";
        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged = (sender, args) => { };
}