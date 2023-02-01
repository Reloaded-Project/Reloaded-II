using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Viewmodel related to the page used to edit the main details of a mod pack.
/// </summary>
public class EditModPackDetailsPageViewModel : ObservableObject
{
    /// <summary>
    /// The pack being modified.
    /// </summary>
    public ObservablePackItem PackItem { get; set; }

    /// <summary/>
    public EditModPackDetailsPageViewModel(ObservablePackItem item)
    {
        PackItem = item;
    }

    /// <summary>
    /// Allows user to select and set readme.
    /// </summary>
    public void SetReadme()
    {
        var result = FileSelectors.SelectMarkdownFile();
        if (!string.IsNullOrEmpty(result))
            PackItem.Readme = File.ReadAllText(result);
    }

    /// <summary>
    /// Adds an image to the given pack.
    /// </summary>
    public void AddImage()
    {
        var result = FileSelectors.SelectImageFile();
        if (!string.IsNullOrEmpty(result))
            PackItem.Images.Add(new ObservablePackImage(new FileStream(result, FileMode.Open), "Default Caption"));
    }

    /// <summary>
    /// Removes image at a given index.
    /// </summary>
    public void RemoveImageAtIndex(int index)
    {
        if (index < PackItem.Images.Count)
            PackItem.Images.RemoveAt(index);
    }

    /// <summary>
    /// Saves readme file onto disk.
    /// </summary>
    public void SaveReadme()
    {
        var saveFilePath = FileSelectors.SelectMarkdownSaveFile();
        if (string.IsNullOrEmpty(saveFilePath))
            return;

        File.WriteAllText(saveFilePath, PackItem.Readme);
    }
}