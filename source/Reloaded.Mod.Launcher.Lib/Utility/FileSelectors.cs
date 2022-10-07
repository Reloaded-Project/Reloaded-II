using PackConstants = Reloaded.Mod.Loader.Update.Packs.Constants;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Class that helps with selecting file from filesystem.
/// </summary>
public static class FileSelectors
{
    /// <summary>
    /// Opens a file selection dialog for selecting a mod configuration.
    /// </summary>
    /// <returns>The path to the selected configuration.</returns>
    public static string SelectModConfigFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.PublishSelectConfigTitle.Get();
        dialog.FileName = ModConfig.ConfigFileName;
        dialog.Filter = $"{Resources.PublishSelectConfigFileTypeName.Get()}|ModConfig.json";
        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }

    /// <summary>
    /// Opens a file selection dialog for selecting a markdown file.
    /// </summary>
    /// <returns>The path to the selected markdown file.</returns>
    public static string SelectMarkdownFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.PublishSelectMarkdownTitle.Get();
        dialog.Filter = $"{Resources.PublishSelectMarkdownTypeName.Get()}|*.md";
        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }

    /// <summary>
    /// Opens a file selection dialog for selecting a Reloaded II mod pack.
    /// </summary>
    /// <returns>The path to the selected mod pack.</returns>
    public static string SelectPackFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.ModPackSelectExistingTitle.Get();
        dialog.Filter = $"{Resources.ModPackSelectExistingTypeName.Get()}|*{PackConstants.Extension}";
        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }

    /// <summary>
    /// Opens a file selection dialog for selecting an image.
    /// </summary>
    /// <returns>The path to the selected image.</returns>
    public static string SelectImageFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.ImageSelectorTitle.Get();
        dialog.Filter = $"{Resources.ImageSelectorFilter.Get()} {Constants.DialogSupportedFormatsFilter}";
        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }

    /// <summary>
    /// Opens a save selection dialog for saving a markdown file.
    /// </summary>
    /// <returns>The path to the selected image.</returns>
    public static string SelectMarkdownSaveFile()
    {
        var dialog = new VistaSaveFileDialog();
        dialog.Title = Resources.PublishSelectMarkdownTitle.Get();
        dialog.Filter = $"{Resources.PublishSelectMarkdownTypeName.Get()}|*.md";
        if ((bool)dialog.ShowDialog()!)
        {
            if (!dialog.FileName.EndsWith(".md"))
                dialog.FileName += ".md";

            return dialog.FileName;
        }

        return "";
    }

    /// <summary>
    /// Opens a save selection dialog for saving a Reloaded pack.
    /// </summary>
    /// <returns>The path to the selected image.</returns>
    public static string SelectPackSaveFile()
    {
        var dialog = new VistaSaveFileDialog();
        dialog.Title = Resources.ModPackSelectExistingTitle.Get();
        dialog.Filter = $"{Resources.ModPackSelectExistingTypeName.Get()}|*{PackConstants.Extension}";
        if ((bool)dialog.ShowDialog()!)
        {
            if (!dialog.FileName.EndsWith(PackConstants.Extension))
                dialog.FileName += PackConstants.Extension;

            return dialog.FileName;
        }

        return "";
    }
}