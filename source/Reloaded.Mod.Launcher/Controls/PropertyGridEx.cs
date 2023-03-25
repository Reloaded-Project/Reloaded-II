using Reloaded.Mod.Interfaces.Structs;
using Color = System.Windows.Media.Color;
using PropertyItem = HandyControl.Controls.PropertyItem;
using TextBox = System.Windows.Controls.TextBox;

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
            default: // if null
                break;
            case SliderControlParamsAttribute:
                var t = (SliderControlParamsAttribute)controlParam;
                return new SliderPropertyEditor(t.Minimum, t.Maximum, t.SmallChange, t.LargeChange, t.TickFrequency, t.IsSnapToTickEnabled, t.TickPlacement, this);
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
    public PropertyResolverEx Owner { get; internal set; }

    public SliderPropertyEditor(
        double minimum,
        double maximum,
        double smallChange,
        double largeChange,
        int tickFrequency,
        bool isSnapToTickEnabled,
        SliderControlTickPlacement sliderControlTickPlacement,
        PropertyResolverEx propertyResolverEx)
    {
        Minimum = minimum;
        Maximum = maximum;
        SmallChange = smallChange;
        LargeChange = largeChange;
        TickFrequency = tickFrequency;
        IsSnapToTickEnabled = isSnapToTickEnabled;
        TickPlacement = sliderControlTickPlacement;
        Owner = propertyResolverEx;
    }

    public double Minimum { get; }
    public double Maximum { get; }
    public double SmallChange { get; }
    public double LargeChange { get; }
    public int TickFrequency { get; }
    public bool IsSnapToTickEnabled { get; }
    public SliderControlTickPlacement TickPlacement { get; }

    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        propertyItem.AttachTooltipAdder(Owner);
        return new Slider
        {
            Minimum = Minimum,
            Maximum = Maximum,
            SmallChange = SmallChange,
            LargeChange = LargeChange,
            TickFrequency = TickFrequency,
            IsSnapToTickEnabled = IsSnapToTickEnabled,
            TickPlacement = SliderTickPlacementEnumConvert.ToTickPlacement(TickPlacement)
        };
    }

    public override DependencyProperty GetDependencyProperty() => Slider.ValueProperty;
}


