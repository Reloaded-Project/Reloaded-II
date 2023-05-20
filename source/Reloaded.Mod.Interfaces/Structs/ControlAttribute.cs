namespace Reloaded.Mod.Interfaces.Structs;

/// <summary>
/// Change the position that the ticks appear for the Slider Control
/// </summary>
public enum SliderControlTickPlacement
{
    None = 0,
    TopLeft = 1,
    BottomRight = 2,
    Both = 3,
}

public interface ICustomControlAttribute { }

[System.AttributeUsage(System.AttributeTargets.Property)]
public class SliderControlParamsAttribute : Attribute, ICustomControlAttribute
{
    /// <summary>
    /// Minimum Value for the slider
    /// </summary>
    public double Minimum { get; }
    /// <summary>
    /// Maximum Value for the slider
    /// </summary>
    public double Maximum { get; }
    /// <summary>
    /// Amount that the slider changes when performing a "Small Change"
    /// For example, a user with a mouse performs a small change when dragging the slider handle
    /// </summary>
    public double SmallChange { get; }
    /// <summary>
    /// Amount that the slider changes when performing a "Large Change"
    /// For example, a user with a mouse performs a large change when clicking on the slider gutter
    /// </summary>
    public double LargeChange { get; }
    /// <summary>
    /// Places a tick every N values
    /// </summary>
    public int TickFrequency { get; }
    /// <summary>
    /// If enabled, the slider will snap to the tick values.
    /// </summary>
    public bool IsSnapToTickEnabled { get; }
    /// <summary>
    /// Determines which side of the slider (none/above/below/both) the ticks will be shown.
    /// </summary>
    public SliderControlTickPlacement TickPlacement { get; }
    /// <summary>
    /// Enables a small text field showing the value of the number to the left of the slider
    /// </summary>
    public bool ShowTextField { get; }
    /// <summary>
    /// Allows the user to edit the text field and manually input data.
    /// If the data is outside of the range of the slider, the slider will clamp to the provided Min/Max range
    /// </summary>
    public bool IsTextFieldEditable { get; }
    /// <summary>
    /// If the text field is editable, when the user edits the field this regex will cause the text input to reject
    /// any characters that do NOT match it.
    /// </summary>
    public string TextValidationRegex { get; }
    /// <summary>
    /// If the text field is visible, apply this format string to the contents. This is recommended for
    /// non-integral sliders otherwise the slider can behave erratically.
    /// </summary>
    public string TextFieldFormat { get; }

    // 2.4.0 BACKCOMPAT OVERLOAD -- DO NOT TOUCH
    public SliderControlParamsAttribute(
    double minimum,
    double maximum,
    double smallChange,
    double largeChange,
    int tickFrequency,
    bool isSnapToTickEnabled,
    SliderControlTickPlacement tickPlacement,
    bool showTextField,
    bool isTextFieldEditable,
    string textValidationRegex
) : this(minimum, maximum, smallChange, largeChange, tickFrequency, isSnapToTickEnabled, tickPlacement, showTextField, isTextFieldEditable, textValidationRegex, textFieldFormat: "")
    {}

    public SliderControlParamsAttribute(
        double minimum = 0.0,
        double maximum = 1.0,
        double smallChange = 0.1,
        double largeChange = 1.0,
        int tickFrequency = 10,
        bool isSnapToTickEnabled = false,
        SliderControlTickPlacement tickPlacement = SliderControlTickPlacement.None,
        bool showTextField = false,
        bool isTextFieldEditable = true,
        string textValidationRegex = ".*",
        string textFieldFormat = ""
    ) {
        Minimum = minimum;
        Maximum = maximum;
        SmallChange = smallChange;
        LargeChange = largeChange;
        TickFrequency = tickFrequency;
        IsSnapToTickEnabled = isSnapToTickEnabled;
        TickPlacement = tickPlacement;
        ShowTextField = showTextField;
        IsTextFieldEditable = isTextFieldEditable;
        TextValidationRegex = textValidationRegex;
        TextFieldFormat = textFieldFormat;
    }
}

/// <summary>
/// <c>FilePickerParams</c> creates a control with both a text box and a button that when clicked will create a OpenFileDialog
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property)]
public class FilePickerParamsAttribute : Attribute, ICustomControlAttribute
{
    /// <summary>
    /// If provided, and the textbox is currently empty, this directory will be the base directory for the File Dialog
    /// </summary>
    public string InitialDirectory { get; }
    /// <summary>
    /// If the InitialDirectory is null, this will be used to initialize the InitialDirectory to a SpecialFolder, which allows
    /// a developer to choose a special folder like My Documents as the start folder.
    /// </summary>
    public Environment.SpecialFolder InitialFolderPath { get; }
    /// <summary>
    /// Text for the button which when clicked opens the OpenFileDialog
    /// </summary>
    public string ChooseFileButtonLabel { get; }
    /// <summary>
    /// If enabled, the user will be able to edit the file path field manually
    /// </summary>
    public bool UserCanEditPathText { get; }
    /// <summary>
    /// Title of the OpenFileDialog window
    /// </summary>
    public string Title { get; }
    /// <summary>
    /// Filter strings for the OpenFileDialog window. See <see>System.Windows.Forms.FileDialog.Filter</see>
    /// </summary>
    public string Filter { get; }
    /// <summary>
    /// Which filter is selected by default
    /// </summary>
    public int FilterIndex { get; }
    /// <summary>
    /// If enabled, the user can select multiple files. This will be stored as full paths to each file separated by a semicolon
    /// </summary>
    public bool Multiselect { get; }
    /// <summary>
    /// Allows the user to select files that have two extensions like .data.txt
    /// </summary>
    public bool SupportMultiDottedExtensions { get; }
    /// <summary>
    /// Displays hidden files in the OpenFileDialog
    /// </summary>
    public bool ShowHiddenFiles { get; }
    /// <summary>
    /// Enables a preview viewer in the OpenFileDialog
    /// </summary>
    public bool ShowPreview { get; }
    /// <summary>
    /// Attempts to open the last used folder path when loading a second time
    /// </summary>
    public bool RestoreDirectory { get; }
    /// <summary>
    /// If enabled, adds the selected folder to the recent files in Explorer
    /// </summary>
    public bool AddToRecent { get; }

    public FilePickerParamsAttribute(
        string initialDirectory = null,
        Environment.SpecialFolder initialFolderPath = Environment.SpecialFolder.Personal,
        string chooseFileButtonLabel = "Choose File",
        bool userCanEditPathText = true,
        string title = "",
        string filter = "All files (*.*)|*.*",
        int filterIndex = 0,
        bool multiselect = false,
        bool supportMultiDottedExtensions = false,
        bool showHiddenFiles = false,
        bool showPreview = false,
        bool restoreDirectory = false,
        bool addToRecent = false
    ) {
        InitialDirectory = initialDirectory != null ? initialDirectory : Environment.GetFolderPath(initialFolderPath);
        InitialFolderPath = initialFolderPath;
        ChooseFileButtonLabel = chooseFileButtonLabel;
        UserCanEditPathText = userCanEditPathText;
        Title = title;
        Filter = filter;
        FilterIndex = filterIndex;
        Multiselect = multiselect;
        SupportMultiDottedExtensions = supportMultiDottedExtensions;
        ShowHiddenFiles = showHiddenFiles;
        ShowPreview = showPreview;
        RestoreDirectory = restoreDirectory;
        AddToRecent = addToRecent;
    }
}

/// <summary>
/// <c>FolderPickerParamsAttribute</c> creates a control with both a text box and a button that when clicked will create
/// a custom Folder Picker Dialog that behaves like a normal OpenFileDialog but locked down to only choosing folders.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property)]
public class FolderPickerParamsAttribute : Attribute, ICustomControlAttribute
{
    /// <summary>
    /// If provided, and the textbox is currently empty, this directory will be the base directory for the Folder Dialog
    /// </summary>
    public string InitialDirectory { get; }
    /// <summary>
    /// If the InitialDirectory is null, this will be used to initialize the InitialDirectory to a SpecialFolder, which allows
    /// a developer to choose a special folder like My Documents as the start folder.
    /// </summary>
    public Environment.SpecialFolder InitialFolderPath { get; }
    /// <summary>
    /// Text for the button which when clicked will open the custom FolderPickerDialog
    /// </summary>
    public string ChooseFolderButtonLabel { get; }
    /// <summary>
    /// Allows the user to edit the text in the associated text box
    /// </summary>
    public bool UserCanEditPathText { get; }
    /// <summary>
    /// Title of the FolderPicker dialog
    /// </summary>
    public string Title { get; }
    /// <summary>
    /// Text of the Ok button inside the FolderPickerDialog
    /// </summary>
    public string OkButtonLabel { get; }
    /// <summary>
    /// Name of the default folder option that will be prepopulated in the open folder dialog window
    /// </summary>
    public string FileNameLabel { get; }
    /// <summary>
    /// If enabled, the user can select multiple files. This will be stored as full paths to each file separated by a semicolon
    /// </summary>
    public bool Multiselect { get; }
    /// <summary>
    /// Ensures that returned items are file system items
    /// </summary>
    public bool ForceFileSystem { get; }

    public FolderPickerParamsAttribute(
        string initialDirectory = null,
        Environment.SpecialFolder initialFolderPath = Environment.SpecialFolder.Personal,
        string chooseFolderButtonLabel = "Choose Folder",
        bool userCanEditPathText = true,
        string title = "",
        string okButtonLabel = "Ok",
        string fileNameLabel = "",
        bool multiSelect = false,
        bool forceFileSystem = false
    ) {
        InitialDirectory = initialDirectory != null ? initialDirectory : Environment.GetFolderPath(InitialFolderPath);
        InitialFolderPath = initialFolderPath;
        ChooseFolderButtonLabel = chooseFolderButtonLabel;
        UserCanEditPathText = userCanEditPathText;
        Title = title;
        OkButtonLabel = okButtonLabel;
        FileNameLabel = fileNameLabel;
        Multiselect = multiSelect;
        ForceFileSystem = forceFileSystem;
    }
}
