using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces.Structs;

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
    public double Minimum { get; }
    public double Maximum { get; }
    public double SmallChange { get; }
    public double LargeChange { get; }
    public int TickFrequency { get; }
    public bool IsSnapToTickEnabled { get; }
    public SliderControlTickPlacement TickPlacement { get; }
    
    public SliderControlParamsAttribute(
        double minimum = 0.0,
        double maximum = 1.0,
        double smallChange = 0.1,
        double largeChange = 1.0,
        int tickFrequency = 10,
        bool isSnapToTickEnabled = false,
        SliderControlTickPlacement tickPlacement = SliderControlTickPlacement.None
    ) {
        Minimum = minimum;
        Maximum = maximum;
        SmallChange = smallChange;
        LargeChange = largeChange;
        TickFrequency = tickFrequency;
        IsSnapToTickEnabled = isSnapToTickEnabled;
        TickPlacement = tickPlacement;
    }
}

[System.AttributeUsage(System.AttributeTargets.Property)]
public class FilePickerParamsAttribute : Attribute, ICustomControlAttribute
{
    public string InitialDirectory { get; }
    public Environment.SpecialFolder InitialFolderPath { get; }
    public string ChooseFileButtonLabel { get; }
    public string Title { get; }
    public string Filter { get; }
    public int FilterIndex { get; }
    public bool Multiselect { get; }
    public bool SupportMultiDottedExtensions { get; }
    public bool ShowHiddenFiles { get; }
    public bool ShowPreview { get; }
    public bool RestoreDirectory { get; }
    public bool AddToRecent { get; }

    public FilePickerParamsAttribute(
        string initialDirectory = null,
        Environment.SpecialFolder initialFolderPath = Environment.SpecialFolder.Personal,
        string chooseFileButtonLabel = "Choose File",
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
        InitialDirectory = initialDirectory == null ? initialDirectory : Environment.GetFolderPath(initialFolderPath);
        InitialFolderPath = initialFolderPath;
        ChooseFileButtonLabel = chooseFileButtonLabel;
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

[System.AttributeUsage(System.AttributeTargets.Property)]
public class FolderPickerParamsAttribute : Attribute, ICustomControlAttribute
{
    public string InitialDirectory { get; }
    public Environment.SpecialFolder InitialFolderPath { get; }
    public string ChooseFolderButtonLabel { get; }
    public string Title { get; }
    public string OkButtonLabel { get; }
    public string FileNameLabel { get; }
    public bool Multiselect { get; }
    public bool ForceFileSystem { get; }

    public FolderPickerParamsAttribute(
        string initialDirectory = null,
        Environment.SpecialFolder initialFolderPath = Environment.SpecialFolder.Personal,
        string chooseFolderButtonLabel = "Choose Folder",
        string title = "",
        string okButtonLabel = "Ok",
        string fileNameLabel = "",
        bool multiSelect = false,
        bool forceFileSystem = false
    ) {
        InitialDirectory = initialDirectory == null ? initialDirectory : Environment.GetFolderPath(InitialFolderPath);
        InitialFolderPath = initialFolderPath;
        ChooseFolderButtonLabel = chooseFolderButtonLabel;
        Title = title;
        OkButtonLabel = okButtonLabel;
        FileNameLabel = fileNameLabel;
        Multiselect = multiSelect;
        ForceFileSystem = forceFileSystem;
    }
}
