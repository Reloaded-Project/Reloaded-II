using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using HandyControl.Controls;
using HandyControl.Tools;
using Reloaded.Mod.Launcher.Lib.Commands.General;
using TextBox = System.Windows.Controls.TextBox;

namespace Reloaded.Mod.Launcher.Controls;

/// <summary>
/// Customised HandyControl PropertyGrid.
/// </summary>
public class PropertyGridEx : PropertyGrid
{
    public override PropertyResolver PropertyResolver { get; } = new PropertyResolverEx();
}

/// <summary>
/// Custom resolver for HandyControl Elements.
/// </summary>
public class PropertyResolverEx : PropertyResolver
{
    public override PropertyEditorBase CreateDefaultEditor(Type type)
    {
        if (type == typeof(string)) return new PlainTextPropertyEditor();
        
        // Numbers
        if (type == typeof(sbyte)) return new NumberPropertyEditor(sbyte.MinValue, sbyte.MaxValue);
        if (type == typeof(byte))  return new NumberPropertyEditor(byte.MinValue, byte.MaxValue);

        if (type == typeof(short)) return new NumberPropertyEditor(short.MinValue, short.MaxValue);
        if (type == typeof(ushort)) return new NumberPropertyEditor(ushort.MinValue, ushort.MaxValue);

        if (type == typeof(int)) return new NumberPropertyEditor(int.MinValue, int.MaxValue);
        if (type == typeof(uint)) return new NumberPropertyEditor(uint.MinValue, uint.MaxValue);

        if (type == typeof(long)) return new NumberPropertyEditor(long.MinValue, long.MaxValue);
        if (type == typeof(ulong)) return new NumberPropertyEditor(ulong.MinValue, ulong.MaxValue);

        if (type == typeof(float)) return new NumberPropertyEditor(float.MinValue, float.MaxValue);
        if (type == typeof(double)) return new NumberPropertyEditor(double.MinValue, double.MaxValue);

        if (type == typeof(bool)) return new SwitchPropertyEditorEx();
        if (type == typeof(DateTime)) return new DateTimePropertyEditor();

        if (type == typeof(ObservableCollection<string>))
        {
            return new StringCollectionEditor();
        }

        if (type.IsSubclassOf(typeof(Enum))) return new EnumPropertyEditor();

        return base.CreateDefaultEditor(type);
    }

    public override bool IsKnownEditorType(Type type)
    {
        if (type == typeof(ObservableCollection<string>))
            return true;

        return base.IsKnownEditorType(type);
    }
}


/// <summary>
/// Extensions helping working with property resolvers.
/// </summary>
public static class PropertyResolverExtensions 
{
    public static object? GetTooltip(this PropertyItem propertyItem)
    {
        if (string.IsNullOrEmpty(propertyItem.Description))
            return null;

        var tooltip = new ToolTip();
        tooltip.Content = propertyItem.Description;
        ToolTipService.SetInitialShowDelay(tooltip, 0);
        return tooltip;
    }
}

public class StringCollectionEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        var dataGrid = new System.Windows.Controls.DataGrid
        {
            ToolTip = propertyItem.GetTooltip(),
            AutoGenerateColumns = false,
            HeadersVisibility = DataGridHeadersVisibility.None
        };

        dataGrid.SetResourceReference(Control.StyleProperty, "DefaultDataGrid");
        dataGrid.Columns.Add(new DataGridTextColumn()
        {
            Binding = new Binding(".")
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
                        var list = (ObservableCollection<string>)dataGrid.ItemsSource;
                        list.Add("New Item");
                    })
                },
                new MenuItem()
                {
                    Header = "Remove",
                    Command = new RelayCommand(o =>
                    {
                        var list = (ObservableCollection<string>)dataGrid.ItemsSource;
                        list.Remove((string) dataGrid.SelectedItem);
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

    public override DependencyProperty GetDependencyProperty() => System.Windows.Controls.ListBox.ItemsSourceProperty;
}

public class PlainTextPropertyEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        return new System.Windows.Controls.TextBox
        {
            IsReadOnly = propertyItem.IsReadOnly,
            ToolTip = propertyItem.GetTooltip()
        };
    }

    public override DependencyProperty GetDependencyProperty() => System.Windows.Controls.TextBox.TextProperty;
}

public class EnumPropertyEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem) => new System.Windows.Controls.ComboBox
    {
        IsEnabled = !propertyItem.IsReadOnly,
        ItemsSource = Enum.GetValues(propertyItem.PropertyType),
        ToolTip = propertyItem.GetTooltip()
    };

    public override DependencyProperty GetDependencyProperty() => Selector.SelectedValueProperty;
}

public class NumberPropertyEditor : PropertyEditorBase
{
    public NumberPropertyEditor() { }

    public NumberPropertyEditor(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public double Minimum { get; set; }

    public double Maximum { get; set; }

    public override FrameworkElement CreateElement(PropertyItem propertyItem) => new NumericUpDown
    {
        IsReadOnly = propertyItem.IsReadOnly,
        Minimum = Minimum,
        Maximum = Maximum,
        ToolTip = propertyItem.GetTooltip()
    };

    public override DependencyProperty GetDependencyProperty() => NumericUpDown.ValueProperty;
}

public class SwitchPropertyEditorEx : SwitchPropertyEditor
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        var result = base.CreateElement(propertyItem);
        result.ToolTip = propertyItem.GetTooltip();
        return result;
    }

    public override DependencyProperty GetDependencyProperty() => ToggleButton.IsCheckedProperty;
}

public class DateTimePropertyEditor : PropertyEditorBase
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem) => new DateTimePicker
    {
        IsEnabled = !propertyItem.IsReadOnly,
        ToolTip = propertyItem.GetTooltip()
    };

    public override DependencyProperty GetDependencyProperty() => DateTimePicker.SelectedDateTimeProperty;
}