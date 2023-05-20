using Reloaded.Mod.Interfaces.Structs;
using DialogResult = System.Windows.Forms.DialogResult;
using Color = System.Windows.Media.Color;
using PropertyItem = HandyControl.Controls.PropertyItem;
using TextBox = System.Windows.Controls.TextBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Reloaded.Mod.Launcher.Controls;

/// <summary>
/// Customised HandyControl PropertyGrid.
/// </summary>
public class PropertyGridEx : PropertyGrid
{
    /// <summary>
    /// Property for current highlighted object.
    /// </summary>
    public static readonly DependencyProperty HoveredItemProperty = DependencyProperty.Register("HoveredItem", typeof(PropertyItem), typeof(PropertyGridEx), new PropertyMetadata());

    private List<PropertyItem> _properties = new List<PropertyItem>();
    private List<PropertyDescriptor> _propertyDescriptors = new List<PropertyDescriptor>();

    protected override PropertyItem CreatePropertyItem(PropertyDescriptor propertyDescriptor, object component, string category,
        int hierarchyLevel)
    {
        ((PropertyResolverEx)PropertyResolver).Descriptor = propertyDescriptor;
        var item = base.CreatePropertyItem(propertyDescriptor, component, category, hierarchyLevel);
        _properties.Add(item);
        _propertyDescriptors.Add(propertyDescriptor);
        return item;
    }

    public override PropertyResolver PropertyResolver { get; }

    /// <summary>
    /// Sets the currently highlighted propertygrid object.
    /// </summary>
    public PropertyItem HoveredItem
    {
        get => (PropertyItem)GetValue(HoveredItemProperty);
        set => SetValue(HoveredItemProperty, value);
    }

    public PropertyGridEx()
    {
        PropertyResolver = new PropertyResolverEx(this);
    }

    /// <summary>
    /// Resets the values of all properties to 0.
    /// </summary>
    public void ResetValues()
    {
        // Try to create new instance and reset it.
        for (var x = 0; x < _properties.Count; x++)
        {
            var property = _properties[x];
            if (property.DefaultValue != null)
                _propertyDescriptors[x].SetValue(property.Value, property.DefaultValue);
        }
    }
}

/// <summary>
/// Custom resolver for HandyControl Elements.
/// </summary>
public class PropertyResolverEx : PropertyResolver
{
    public PropertyGridEx PropertyGrid { get; set; }
    public PropertyDescriptor? Descriptor { get; set; }

    public PropertyResolverEx(PropertyGridEx grid)
    {
        PropertyGrid = grid;
    }

    public override PropertyEditorBase CreateDefaultEditor(Type type)
    {
        // First check for a user supplied control type
        var controlParam = Descriptor?.Attributes.OfType<ICustomControlAttribute>().FirstOrDefault();
        switch (controlParam)
        {
            default: // if null use the default editor
                break;
            case SliderControlParamsAttribute t:
                return new SliderPropertyEditor(t, this);
            case FilePickerParamsAttribute t:
                return new FilePropertyEditor(t, this);
            case FolderPickerParamsAttribute t:
                return new FolderPropertyEditor(t, this);
        }

        if (type == typeof(string)) return new PlainTextPropertyEditor(this);
        
        // Numbers
        if (type == typeof(sbyte)) return new NumberPropertyEditor(sbyte.MinValue, sbyte.MaxValue, this);
        if (type == typeof(byte))  return new NumberPropertyEditor(byte.MinValue, byte.MaxValue, this);

        if (type == typeof(short)) return new NumberPropertyEditor(short.MinValue, short.MaxValue, this);
        if (type == typeof(ushort)) return new NumberPropertyEditor(ushort.MinValue, ushort.MaxValue, this);

        if (type == typeof(int)) return new NumberPropertyEditor(int.MinValue, int.MaxValue, this);
        if (type == typeof(uint)) return new NumberPropertyEditor(uint.MinValue, uint.MaxValue, this);

        if (type == typeof(long)) return new NumberPropertyEditor(long.MinValue, long.MaxValue, this);
        if (type == typeof(ulong)) return new NumberPropertyEditor(ulong.MinValue, ulong.MaxValue, this);

        if (type == typeof(float)) return new NumberPropertyEditor(float.MinValue, float.MaxValue, this);
        if (type == typeof(double)) return new NumberPropertyEditor(double.MinValue, double.MaxValue, this);

        if (type == typeof(bool)) return new SwitchPropertyEditorEx(this);
        if (type == typeof(DateTime)) return new DateTimePropertyEditor(this);

        if (type == typeof(ObservableCollection<StringWrapper>))
        {
            return new StringWrapperEditor(this);
        }

        if (type.IsSubclassOf(typeof(Enum))) return new EnumPropertyEditor(this);

        return base.CreateDefaultEditor(type);
    }

    public override bool IsKnownEditorType(Type type)
    {
        if (type == typeof(ObservableCollection<StringWrapper>))
            return true;

        return base.IsKnownEditorType(type);
    }
}


/// <summary>
/// Extensions helping working with property resolvers.
/// </summary>
public static class PropertyResolverExtensions 
{
    public static void AttachTooltipAdder(this PropertyItem propertyItem, PropertyResolverEx resolverEx)
    {
        propertyItem.Loaded += (sender, args) => { PropertyItemLoaded(sender, args, resolverEx, !string.IsNullOrEmpty(propertyItem.Description)); };
    }

    private static void PropertyItemLoaded(object? sender, EventArgs e, PropertyResolverEx resolverEx, bool hasDescription)
    {
        var propertyItem = (PropertyItem)sender!;
        
        // Make the child textbox not use built-in tooltip
        var textbox = FindChild<TextBox>(propertyItem, "");
        if (textbox != null)
        {
            textbox.ToolTip = null;
            textbox.Focusable = false;
        }

        // Make the parent expander non-focusable for controllers.
        var expander = WpfUtilities.FindParent<Expander>(propertyItem);
        if (expander != null)
            expander.Focusable = false;

        if (!hasDescription)
            return;

        var tooltip = new ToolTip();
        tooltip.DataContext = propertyItem;
        tooltip.Content = propertyItem.Description;
        tooltip.PlacementTarget = propertyItem;
        tooltip.SetResourceReference(FrameworkElement.StyleProperty, "PropertyGridTooltip");
        ToolTipService.SetInitialShowDelay(tooltip, 0);
        ToolTipService.SetBetweenShowDelay(tooltip, 0);
        propertyItem.ToolTip = tooltip;
        propertyItem.MouseEnter += (o, args) => { PropertyItemOnMouseEnter(o, args, resolverEx); };
        propertyItem.MouseLeave += PropertyItemOnMouseLeave;

        // Set grid colour to transparent so it's hit testable and can pick up mouse events
        var groupbox = FindChild<Grid>(propertyItem, "");
        if (groupbox != null)
            groupbox.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
    }

    private static void PropertyItemOnMouseEnter(object sender, MouseEventArgs e, PropertyResolverEx resolverEx)
    {
        var propertyItem = (PropertyItem)sender;
        resolverEx.PropertyGrid.HoveredItem = propertyItem;
        if (propertyItem.ToolTip is ToolTip tooltip)
            tooltip.IsOpen = true;
    }

    private static void PropertyItemOnMouseLeave(object sender, MouseEventArgs e)
    {
        var propertyItem = (PropertyItem)sender;
        if (propertyItem.ToolTip is ToolTip tooltip)
            tooltip.IsOpen = false;
    }

    /// <summary>
    /// Finds a Child of a given item in the visual tree. 
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="childName">x:Name or Name of child. </param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static T? FindChild<T>(DependencyObject? parent, string? childName)
        where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null) 
            return null;

        T? foundChild = null;
        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // If the child is not of the request child type child
            T? childType = child as T;
            if (childType == null)
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null) 
                    break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                // If the child's name is set for search
                if (child is not FrameworkElement frameworkElement || frameworkElement.Name != childName) 
                    continue;

                // if the child's name is of the request name
                foundChild = (T)child;
                break;
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }
}

public class StringWrapperEditor : PropertyEditorBase
{
    public PropertyResolverEx Owner { get; internal set; }

    public StringWrapperEditor(PropertyResolverEx propertyResolverEx) => Owner = propertyResolverEx;

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            HeadersVisibility = DataGridHeadersVisibility.None,
            CanUserAddRows = false,
        };

        dataGrid.SetResourceReference(FrameworkElement.StyleProperty, "DefaultDataGrid");
        dataGrid.Columns.Add(new DataGridTextColumn()
        {
            Binding = new Binding(nameof(StringWrapper.Value)) { Mode = BindingMode.TwoWay }
        });

        dataGrid.ContextMenu = new ContextMenu()
        {
            Items =
            {
                new MenuItem()
                {
                    Header = "Add",
                    Command = new RelayCommand(o =>
                    {
                        var list = (ObservableCollection<StringWrapper>)dataGrid.ItemsSource;
                        list.Add("New Item");
                    })
                },
                new MenuItem()
                {
                    Header = "Remove",
                    Command = new RelayCommand(o =>
                    {
                        var list = (ObservableCollection<StringWrapper>)dataGrid.ItemsSource;
                        if (dataGrid.SelectedItem is StringWrapper wrapper)
                            list.Remove(wrapper);
                    }, o =>
                    {
                        return dataGrid.SelectedItem != null;
                    })
                },
            }
        };

        dataGrid.SelectionChanged += (sender, args) =>
        {
            CommandManager.InvalidateRequerySuggested();
        };

        return dataGrid;
    }

    public override DependencyProperty GetDependencyProperty() => ItemsControl.ItemsSourceProperty;
}

public class PlainTextPropertyEditor : PropertyEditorBase
{
    public PropertyResolverEx Owner { get; internal set; }

    public PlainTextPropertyEditor(PropertyResolverEx propertyResolverEx) => Owner = propertyResolverEx;

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        return new TextBox
        {
            IsReadOnly = propertyItem.IsReadOnly,
        };
    }

    public override DependencyProperty GetDependencyProperty() => TextBox.TextProperty;
}

public class EnumPropertyEditor : PropertyEditorBase
{
    public PropertyResolverEx Owner { get; internal set; }

    public EnumPropertyEditor(PropertyResolverEx propertyResolverEx) => Owner = propertyResolverEx;

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        return new System.Windows.Controls.ComboBox
        {
            IsEnabled = !propertyItem.IsReadOnly,
            ItemsSource = Enum.GetValues(propertyItem.PropertyType)
        };
    }

    public override DependencyProperty GetDependencyProperty() => Selector.SelectedValueProperty;
}

public class NumberPropertyEditor : PropertyEditorBase
{
    public PropertyResolverEx Owner { get; internal set; }

    public NumberPropertyEditor(double minimum, double maximum, PropertyResolverEx propertyResolverEx)
    {
        Minimum = minimum;
        Maximum = maximum;
        Owner = propertyResolverEx;
    }

    public double Minimum { get; set; }

    public double Maximum { get; set; }

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        return new NumericUpDown
        {
            IsReadOnly = propertyItem.IsReadOnly,
            Minimum = Minimum,
            Maximum = Maximum
        };
    }

    public override DependencyProperty GetDependencyProperty() => NumericUpDown.ValueProperty;
}

public class SwitchPropertyEditorEx : SwitchPropertyEditor
{
    public PropertyResolverEx Owner { get; internal set; }

    public SwitchPropertyEditorEx(PropertyResolverEx propertyResolverEx) => Owner = propertyResolverEx;

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        var result = base.CreateElement(propertyItem);
        result.SetResourceReference(FrameworkElement.FocusVisualStyleProperty, "ReloadedFocusVisual");
        propertyItem.AttachTooltipAdder(Owner);
        return result;
    }

    public override DependencyProperty GetDependencyProperty() => ToggleButton.IsCheckedProperty;
}

public class DateTimePropertyEditor : PropertyEditorBase
{
    public PropertyResolverEx Owner { get; internal set; }
    public DateTimePropertyEditor(PropertyResolverEx propertyResolverEx) => Owner = propertyResolverEx;

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        return new DateTimePicker
        {
            IsEnabled = !propertyItem.IsReadOnly
        };
    }

    public override DependencyProperty GetDependencyProperty() => DateTimePicker.SelectedDateTimeProperty;
}

public static class SliderTickPlacementEnumConvert
{
    public static TickPlacement ToTickPlacement(SliderControlTickPlacement tickPlacement)
    {
        switch (tickPlacement)
        {
            default: return TickPlacement.None;
            case SliderControlTickPlacement.None: return TickPlacement.None;
            case SliderControlTickPlacement.TopLeft: return TickPlacement.TopLeft;
            case SliderControlTickPlacement.BottomRight: return TickPlacement.BottomRight;
            case SliderControlTickPlacement.Both: return TickPlacement.Both;
        }
    }
}

public class SliderPropertyEditor : PropertyEditorBase
{
    public SliderControlParamsAttribute SliderControlParams { get; }
    public PropertyResolverEx Owner { get; internal set; }

    private Slider? _slider;

    public SliderPropertyEditor(
        SliderControlParamsAttribute sliderControlParams,
        PropertyResolverEx propertyResolverEx)
    {
        SliderControlParams = sliderControlParams;
        Owner = propertyResolverEx;
    }

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);

        var panel = new DockPanel();
        _slider = new Slider
        {
            Minimum = SliderControlParams.Minimum,
            Maximum = SliderControlParams.Maximum,
            SmallChange = SliderControlParams.SmallChange,
            LargeChange = SliderControlParams.LargeChange,
            TickFrequency = SliderControlParams.TickFrequency,
            IsSnapToTickEnabled = SliderControlParams.IsSnapToTickEnabled,
            TickPlacement = SliderTickPlacementEnumConvert.ToTickPlacement(SliderControlParams.TickPlacement)
        };

        if (SliderControlParams.ShowTextField)
        {
            var textbox = new TextBox
            {
                IsReadOnly = !SliderControlParams.IsTextFieldEditable,
                IsEnabled = SliderControlParams.IsTextFieldEditable,
                MinWidth = 10,
                Margin = new Thickness(0, 0, 5, 0),
            };
            textbox.SetBinding(TextBox.TextProperty, new Binding()
            {
                Source = _slider,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = SliderControlParams.TextFieldFormat ?? ""
            });
            if (SliderControlParams.TextValidationRegex != null)
            {
                var validate = new System.Text.RegularExpressions.Regex(SliderControlParams.TextValidationRegex);
                textbox.PreviewTextInput += (object sender, TextCompositionEventArgs e) =>
                {
                    // Prevent the event from processing any further
                    e.Handled = !validate.IsMatch(e.Text);
                };
            }
            DockPanel.SetDock(textbox, Dock.Left);
            panel.Children.Add(textbox);
        }
        DockPanel.SetDock(_slider, Dock.Right);
        panel.Children.Add(_slider);
        return panel;
    }

    public override void CreateBinding(PropertyItem propertyItem, DependencyObject element)
    {
        BindingOperations.SetBinding(_slider, Slider.ValueProperty, new Binding(propertyItem.PropertyName ?? "")
        {
            Source = propertyItem.Value,
            Mode = GetBindingMode(propertyItem),
            UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
            Converter = GetConverter(propertyItem)
        });
    }

    // Since we override CreateBinding directly, this is unused but still needs overridden.
    public override DependencyProperty GetDependencyProperty() => throw new NotImplementedException();
}

public abstract class PathPropertyEditor : PropertyEditorBase
{
    public PropertyResolverEx Owner { get; internal set; }
    public string ButtonLabel { get; }
    public bool CanEditPathText { get; }
    protected TextBox? _textbox;

    public PathPropertyEditor(string buttonLabel, bool canEditPathText, PropertyResolverEx propertyResolverEx)
    {
        Owner = propertyResolverEx;
        ButtonLabel = buttonLabel;
        CanEditPathText = canEditPathText;
    }

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        var panel = new DockPanel();

        _textbox = new TextBox {
            IsReadOnly = !CanEditPathText,
            IsEnabled = CanEditPathText,
            Margin = new Thickness(0, 0, 5, 0),
        };

        var button = new System.Windows.Controls.Button {
            Style = (Style)panel.FindResource("ButtonPrimary"),
            Content = ButtonLabel,
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        {
            if (ShowDialog() == DialogResult.OK)
            {
                _textbox.Text = GetResult();
                // Scroll the position of the text in the textbox to the end in case the path is long
                // so we focus on the final file/folder in the path instead
                _textbox.ScrollToHorizontalOffset(_textbox.ExtentWidth);
            }
        };

        DockPanel.SetDock(button, Dock.Right);
        panel.Children.Add(button);
        DockPanel.SetDock(_textbox, Dock.Left);
        panel.Children.Add(_textbox);
        return panel;
    }

    protected abstract DialogResult ShowDialog();

    protected abstract string GetResult();

    public override void CreateBinding(PropertyItem propertyItem, DependencyObject element)
    {
        BindingOperations.SetBinding(_textbox, TextBox.TextProperty, new Binding(propertyItem.PropertyName ?? "")
        {
            Source = propertyItem.Value,
            Mode = GetBindingMode(propertyItem),
            UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
            Converter = GetConverter(propertyItem)
        });
    }

    // Since we override CreateBinding directly, this is unused but still needs overridden.
    public override DependencyProperty GetDependencyProperty() => throw new NotImplementedException();
}

public class FilePropertyEditor : PathPropertyEditor
{
    private OpenFileDialog? _openFileDialog;
    private FilePickerParamsAttribute _filePickerParams { get; }

    public FilePropertyEditor(FilePickerParamsAttribute filePickerParams, PropertyResolverEx propertyResolverEx)
        : base(filePickerParams.ChooseFileButtonLabel, filePickerParams.UserCanEditPathText, propertyResolverEx)
    {
        _filePickerParams = filePickerParams;
    }

    protected override DialogResult ShowDialog()
    {
        var initPath = !string.IsNullOrEmpty(_textbox.Text) ? _textbox.Text : _filePickerParams.InitialDirectory;
        _openFileDialog = new OpenFileDialog
        {
            Filter = _filePickerParams.Filter,
            InitialDirectory = initPath,
            Title = _filePickerParams.Title,
            FilterIndex = _filePickerParams.FilterIndex,
            Multiselect = _filePickerParams.Multiselect,
            SupportMultiDottedExtensions = _filePickerParams.SupportMultiDottedExtensions,
            ShowHiddenFiles = _filePickerParams.ShowHiddenFiles,
            RestoreDirectory = _filePickerParams.RestoreDirectory,
            AddToRecent = _filePickerParams.AddToRecent,
            ShowPreview = _filePickerParams.ShowPreview
        };
        return _openFileDialog.ShowDialog();
    }

    protected override string GetResult()
    {
        if (_filePickerParams.Multiselect)
            return string.Join(";", _openFileDialog!.FileNames);
        else
            return _openFileDialog!.FileName;
    }
}

public class FolderPropertyEditor : PathPropertyEditor
{
    private FolderPicker? _folderPicker;
    private FolderPickerParamsAttribute _folderPickerParams { get; }

    public FolderPropertyEditor(FolderPickerParamsAttribute folderPickerParams, PropertyResolverEx propertyResolverEx)
        : base(folderPickerParams.ChooseFolderButtonLabel, folderPickerParams.UserCanEditPathText, propertyResolverEx)
    {
        _folderPickerParams = folderPickerParams;
    }

    protected override DialogResult ShowDialog()
    {
        var window = System.Windows.Window.GetWindow(Owner.PropertyGrid);
        var initPath = !string.IsNullOrEmpty(_textbox.Text) ? _textbox.Text : _folderPickerParams.InitialDirectory;
        _folderPicker = new FolderPicker
        {
            InputPath = initPath,
            Title = _folderPickerParams.Title,
            OkButtonLabel = _folderPickerParams.OkButtonLabel,
            FileNameLabel = _folderPickerParams.FileNameLabel,
            Multiselect = _folderPickerParams.Multiselect,
            ForceFileSystem = _folderPickerParams.ForceFileSystem,
        };
        return _folderPicker.ShowDialog(window);
    }

    protected override string GetResult()
    {
        if (_folderPickerParams.Multiselect)
            return string.Join(";", _folderPicker!.ResultNames);
        else
            return _folderPicker!.ResultName;
    }
}

/**
 * C# WPF FileOpenDialog doesn't have folder support by default, and the OpenFolderDialog is notoriously
 * poorly implemented, so this custom dialog uses the FileOpenDialog but sets normally unavaiable options
 * through the COM interface.
 * 
 * Folder Picker without external dependencies from Simon Mourier https://stackoverflow.com/a/66187224
 * License is CC BY-SA 4.0 https://creativecommons.org/licenses/by-sa/4.0/
 * Modified slightly to remove a few unused imports, changed return type to DialogResult, added default empty path/name
 */
public class FolderPicker
{
    private readonly List<string> _resultPaths = new List<string>();
    private readonly List<string> _resultNames = new List<string>();

    public IReadOnlyList<string> ResultPaths => _resultPaths;
    public IReadOnlyList<string> ResultNames => _resultNames;
    public string ResultPath => ResultPaths.FirstOrDefault("");
    public string ResultName => ResultNames.FirstOrDefault("");
    public virtual string? InputPath { get; set; }
    public virtual bool ForceFileSystem { get; set; }
    public virtual bool Multiselect { get; set; }
    public virtual string? Title { get; set; }
    public virtual string? OkButtonLabel { get; set; }
    public virtual string? FileNameLabel { get; set; }

    protected virtual int SetOptions(int options)
    {
        if (ForceFileSystem)
        {
            options |= (int)FOS.FOS_FORCEFILESYSTEM;
        }

        if (Multiselect)
        {
            options |= (int)FOS.FOS_ALLOWMULTISELECT;
        }
        return options;
    }

    // for WPF support
    public DialogResult ShowDialog(System.Windows.Window owner = null, bool throwOnError = false)
    {
        owner = owner ?? Application.Current?.MainWindow;
        return ShowDialog(owner != null ? new System.Windows.Interop.WindowInteropHelper(owner).Handle : IntPtr.Zero, throwOnError);
    }

    // for all .NET
    public virtual DialogResult ShowDialog(IntPtr owner, bool throwOnError = false)
    {
        var dialog = (IFileOpenDialog)new FileOpenDialog();
        if (!string.IsNullOrEmpty(InputPath))
        {
            if (CheckHr(SHCreateItemFromParsingName(InputPath, IntPtr.Zero, typeof(IShellItem).GUID, out var item), throwOnError) != 0)
                return DialogResult.Cancel;

            dialog.SetFolder(item);
        }

        var options = FOS.FOS_PICKFOLDERS;
        options = (FOS)SetOptions((int)options);
        dialog.SetOptions(options);

        if (Title != null)
        {
            dialog.SetTitle(Title);
        }

        if (OkButtonLabel != null)
        {
            dialog.SetOkButtonLabel(OkButtonLabel);
        }

        if (FileNameLabel != null)
        {
            dialog.SetFileName(FileNameLabel);
        }

        if (owner == IntPtr.Zero)
        {
            owner = Process.GetCurrentProcess().MainWindowHandle;
            if (owner == IntPtr.Zero)
            {
                owner = GetDesktopWindow();
            }
        }

        var hr = dialog.Show(owner);
        if (hr == ERROR_CANCELLED)
            return DialogResult.Cancel;

        if (CheckHr(hr, throwOnError) != 0)
            return DialogResult.Cancel;

        if (CheckHr(dialog.GetResults(out var items), throwOnError) != 0)
            return DialogResult.Cancel;

        items.GetCount(out var count);
        for (var i = 0; i < count; i++)
        {
            items.GetItemAt(i, out var item);
            CheckHr(item.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out var path), throwOnError);
            CheckHr(item.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out var name), throwOnError);
            if (path != null || name != null)
            {
                _resultPaths.Add(path);
                _resultNames.Add(name);
            }
        }
        return DialogResult.OK;
    }

    private static int CheckHr(int hr, bool throwOnError)
    {
        if (hr != 0 && throwOnError) Marshal.ThrowExceptionForHR(hr);
        return hr;
    }

    [DllImport("shell32")]
    private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

    [DllImport("user32")]
    private static extern IntPtr GetDesktopWindow();

#pragma warning disable IDE1006 // Naming Styles
    private const int ERROR_CANCELLED = unchecked((int)0x800704C7);
#pragma warning restore IDE1006 // Naming Styles

    [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")] // CLSID_FileOpenDialog
    private class FileOpenDialog { }

    [ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        [PreserveSig] int Show(IntPtr parent); // IModalWindow
        [PreserveSig] int SetFileTypes();  // not fully defined
        [PreserveSig] int SetFileTypeIndex(int iFileType);
        [PreserveSig] int GetFileTypeIndex(out int piFileType);
        [PreserveSig] int Advise(); // not fully defined
        [PreserveSig] int Unadvise();
        [PreserveSig] int SetOptions(FOS fos);
        [PreserveSig] int GetOptions(out FOS pfos);
        [PreserveSig] int SetDefaultFolder(IShellItem psi);
        [PreserveSig] int SetFolder(IShellItem psi);
        [PreserveSig] int GetFolder(out IShellItem ppsi);
        [PreserveSig] int GetCurrentSelection(out IShellItem ppsi);
        [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [PreserveSig] int GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
        [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        [PreserveSig] int GetResult(out IShellItem ppsi);
        [PreserveSig] int AddPlace(IShellItem psi, int alignment);
        [PreserveSig] int SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        [PreserveSig] int Close(int hr);
        [PreserveSig] int SetClientGuid();  // not fully defined
        [PreserveSig] int ClearClientData();
        [PreserveSig] int SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);
        [PreserveSig] int GetResults(out IShellItemArray ppenum);
        [PreserveSig] int GetSelectedItems([MarshalAs(UnmanagedType.IUnknown)] out object ppsai);
    }

    [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        [PreserveSig] int BindToHandler(); // not fully defined
        [PreserveSig] int GetParent(); // not fully defined
        [PreserveSig] int GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
        [PreserveSig] int GetAttributes();  // not fully defined
        [PreserveSig] int Compare();  // not fully defined
    }

    [ComImport, Guid("b63ea76d-1f85-456f-a19c-48159efa858b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemArray
    {
        [PreserveSig] int BindToHandler();  // not fully defined
        [PreserveSig] int GetPropertyStore();  // not fully defined
        [PreserveSig] int GetPropertyDescriptionList();  // not fully defined
        [PreserveSig] int GetAttributes();  // not fully defined
        [PreserveSig] int GetCount(out int pdwNumItems);
        [PreserveSig] int GetItemAt(int dwIndex, out IShellItem ppsi);
        [PreserveSig] int EnumItems();  // not fully defined
    }

#pragma warning disable CA1712 // Do not prefix enum values with type name
    private enum SIGDN : uint
    {
        SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
        SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
        SIGDN_FILESYSPATH = 0x80058000,
        SIGDN_NORMALDISPLAY = 0,
        SIGDN_PARENTRELATIVE = 0x80080001,
        SIGDN_PARENTRELATIVEEDITING = 0x80031001,
        SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
        SIGDN_PARENTRELATIVEPARSING = 0x80018001,
        SIGDN_URL = 0x80068000
    }

    [Flags]
    private enum FOS
    {
        FOS_OVERWRITEPROMPT = 0x2,
        FOS_STRICTFILETYPES = 0x4,
        FOS_NOCHANGEDIR = 0x8,
        FOS_PICKFOLDERS = 0x20,
        FOS_FORCEFILESYSTEM = 0x40,
        FOS_ALLNONSTORAGEITEMS = 0x80,
        FOS_NOVALIDATE = 0x100,
        FOS_ALLOWMULTISELECT = 0x200,
        FOS_PATHMUSTEXIST = 0x800,
        FOS_FILEMUSTEXIST = 0x1000,
        FOS_CREATEPROMPT = 0x2000,
        FOS_SHAREAWARE = 0x4000,
        FOS_NOREADONLYRETURN = 0x8000,
        FOS_NOTESTFILECREATE = 0x10000,
        FOS_HIDEMRUPLACES = 0x20000,
        FOS_HIDEPINNEDPLACES = 0x40000,
        FOS_NODEREFERENCELINKS = 0x100000,
        FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
        FOS_DONTADDTORECENT = 0x2000000,
        FOS_FORCESHOWHIDDEN = 0x10000000,
        FOS_DEFAULTNOMINIMODE = 0x20000000,
        FOS_FORCEPREVIEWPANEON = 0x40000000,
        FOS_SUPPORTSTREAMABLEITEMS = unchecked((int)0x80000000)
    }
#pragma warning restore CA1712 // Do not prefix enum values with type name
}
