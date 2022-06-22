using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Reloaded.Mod.Launcher.Controls.Properties;

/// <summary>
/// An attached WPF property that allows for a tooltip to follow the mouse.
/// </summary>
public class MoveTooltipWithMouse : WPF.MVVM.AttachedPropertyBase<MoveTooltipWithMouse, bool>
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(MoveTooltipWithMouse), new UIPropertyMetadata(true));
    public static readonly DependencyProperty OffsetXProperty = DependencyProperty.RegisterAttached("OffsetX", typeof(int), typeof(MoveTooltipWithMouse), new UIPropertyMetadata(0));
    public static readonly DependencyProperty OffsetYProperty = DependencyProperty.RegisterAttached("OffsetY", typeof(int), typeof(MoveTooltipWithMouse), new UIPropertyMetadata(0));
    
    public static bool GetIsEnabled(DependencyObject dependencyObject) => (bool)dependencyObject.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject dependencyObject, bool value) => dependencyObject.SetValue(IsEnabledProperty, value);

    public static int GetOffsetX(DependencyObject dependencyObject) => (int)dependencyObject.GetValue(OffsetXProperty);
    public static void SetOffsetX(DependencyObject dependencyObject, int value) => dependencyObject.SetValue(OffsetXProperty, value);

    public static int GetOffsetY(DependencyObject dependencyObject) => (int)dependencyObject.GetValue(OffsetYProperty);
    public static void SetOffsetY(DependencyObject dependencyObject, int value) => dependencyObject.SetValue(OffsetYProperty, value);

    public const int OffsetX = 15;

    public override void OnValueUpdated(DependencyObject sender, object value)
    {
        if ((bool)value != true) 
            return;

        // If attached to tooltip, handle via parent.
        if (sender is ToolTip tooltip)
            OnTooltipUpdated(tooltip);

        // Otherwise check for tooltip on given element
        if (sender is FrameworkElement element && element.ToolTip is ToolTip elementTooltip)
            OnTooltipUpdated(elementTooltip);
    }

    private void OnTooltipUpdated(ToolTip tooltip) => tooltip.Loaded += InitMouseTrack;

    private void InitMouseTrack(object sender, RoutedEventArgs e)
    {
        var tooltip = (ToolTip)sender;
        
        if (tooltip.PlacementTarget is FrameworkElement element)
        {
            // We have to be VERY CAREFUL here.
            // If the Attached Property is declared as part of a style, then
            // the instance of MoveTooltipWithMouse will be shared with all instances.  

            // In this case, the MoveTooltipWithMouse will effectively last the lifetime of the process.  
            // Subscribing to `element.MouseMove` injects a reference to MoveTooltipWithMouse into the 
            // element, meaning it will never get garbage collected!!  

            // Therefore, to avoid memory leak, we need to unsubscribe as the tooltip is unloaded.  

            void OnElementMouseMove(object sender, MouseEventArgs args) => OnMouseMove(sender, tooltip, args);
            void OnTooltipUnloaded(object o, RoutedEventArgs args)
            {
                // Remove references to MoveTooltipWithMouse
                var tip = ((ToolTip)o);
                element.MouseMove -= OnElementMouseMove;
                tip.Unloaded -= OnTooltipUnloaded; 
            }

            element.MouseMove += OnElementMouseMove;
            tooltip.Unloaded += OnTooltipUnloaded;
            InitTooltip(element, tooltip);
        }

        tooltip.Loaded -= InitMouseTrack;
    }

    private void InitTooltip(FrameworkElement element, ToolTip tooltip)
    {
        if (!(bool)tooltip.GetValue(IsEnabledProperty))
            return;
        
        if (element.IsMouseOver)
            SetMousePosition(tooltip, element, Mouse.GetPosition(element));
    }

    private void OnMouseMove(object sender, ToolTip tooltip, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element) 
            return;

        if (!(bool)tooltip.GetValue(IsEnabledProperty)) 
            return;
        
        SetMousePosition(tooltip, element, e.GetPosition(element));
    }

    private static void SetMousePosition(ToolTip tooltip, FrameworkElement parent, Point currentPos)
    {
        tooltip.Placement = PlacementMode.Relative;
        tooltip.HorizontalOffset = currentPos.X + OffsetX + (int)tooltip.GetValue(OffsetXProperty) + (int)parent.GetValue(OffsetXProperty);
        tooltip.VerticalOffset = currentPos.Y + (int)tooltip.GetValue(OffsetYProperty) + (int)parent.GetValue(OffsetYProperty);
    }
}