using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Viewmodel related to the page used to edit the main details of a mod pack.
/// </summary>
public class EditMainPackDetailsPageViewModel : ObservableObject
{
    /// <summary>
    /// The pack being modified.
    /// </summary>
    public ObservablePack Pack { get; set; } = new();

    /// <summary/>
    public EditMainPackDetailsPageViewModel(ObservablePack pack)
    {
        Pack = pack;
    }

    /// <summary>
    /// Allows user to select and set readme.
    /// </summary>
    public void SetReadme()
    {
        var result = FileSelectors.SelectMarkdownFile();
        if (!string.IsNullOrEmpty(result))
            Pack.Readme = File.ReadAllText(result);
    }

    /// <summary>
    /// Adds an image to the given pack.
    /// </summary>
    public void AddImage()
    {
        var result = FileSelectors.SelectImageFile();
        if (!string.IsNullOrEmpty(result))
            Pack.Images.Add(new ObservablePackImage(new FileStream(result, FileMode.Open), "Default Caption"));
    }

    /// <summary>
    /// Removes image at a given index.
    /// </summary>
    public void RemoveImageAtIndex(int index)
    {
        if (index < Pack.Images.Count)
            Pack.Images.RemoveAt(index);
    }
}