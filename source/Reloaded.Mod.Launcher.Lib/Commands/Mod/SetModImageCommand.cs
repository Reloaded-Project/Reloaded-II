namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command to set a new image for a specified mod.
/// </summary>
public class SetModImageCommand : ICommand
{
    private PathTuple<ModConfig>? _modTuple;

    /// <summary/>
    public SetModImageCommand(PathTuple<ModConfig>? modTuple) => _modTuple = modTuple;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _modTuple != null;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        // Select image
        string imagePath = SelectImageFile();

        if (String.IsNullOrEmpty(imagePath) || ! File.Exists(imagePath))
            return;

        // Get selected item.
        string modDirectory = Path.GetDirectoryName(_modTuple!.Path)!;

        // Set icon name and save.
        string iconFileName = Path.GetFileName(imagePath);
        if (modDirectory != null)
        {
            string iconPath = Path.Combine(modDirectory, iconFileName);

            // Copy image and set config file path.
            if (!imagePath.Equals(iconPath, StringComparison.OrdinalIgnoreCase))
                File.Copy(imagePath, iconPath, true);

            _modTuple.Config.ModIcon = iconFileName;
            _modTuple.Save();
        }
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged = (sender, args) => { };

    /// <summary>
    /// Opens up a file selection dialog allowing for the selection of a custom image to associate with the profile.
    /// </summary>
    private string SelectImageFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.CreateModDialogImageSelectorTitle.Get();
        dialog.Filter = $"{Resources.CreateModDialogImageSelectorFilter.Get()} {Constants.DialogSupportedFormatsFilter}";
        if ((bool) dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }
}