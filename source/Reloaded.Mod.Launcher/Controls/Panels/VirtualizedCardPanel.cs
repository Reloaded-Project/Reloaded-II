using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Reloaded.Mod.Launcher.Controls.Panels;

/// <summary>
/// Panel that accepts a number of equally sized children (using the first child as reference for size).
/// Stretches the results horizontally, and wraps around after a de
/// </summary>
public class VirtualizedCardPanel : Panel
{
    /// <summary/>
    public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register(nameof(ColumnWidth), typeof(double), typeof(VirtualizedCardPanel), new PropertyMetadata(default(double)));
    
    /// <summary/>
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth), typeof(double), typeof(VirtualizedCardPanel), new PropertyMetadata(default(double)));

    /// <summary/>
    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight), typeof(double), typeof(VirtualizedCardPanel), new PropertyMetadata(default(double)));

    /// <summary>
    /// Sets the height of each individual child item.
    /// </summary>
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>
    /// Sets the width of each individual child item.
    /// </summary>
    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Desired width of each individual column.
    /// </summary>
    public double ColumnWidth
    {
        get => (double)GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    // Position child element, return actual used size.
    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = InternalChildren;
        var childrenCount = children.Count;
        var columns  = ColumnCount(finalSize);
        var elementWidth = finalSize.Width / columns;
        var childSize = new Size(elementWidth, ItemHeight);

        for (int x = 0; x < childrenCount; x++)
        {
            var row   = x / columns;
            var remainder = x % columns;

            var child = InternalChildren[x];
            var pos = new Point(remainder * childSize.Width, row * childSize.Height);
            child.Arrange(new Rect(pos, childSize));
        }

        return new Size(finalSize.Width, RowCount(children, columns) * ItemHeight);
    }

    // Measure size in layout required for child elements.
    protected override Size MeasureOverride(Size availableSize)
    {
        // We need to access children first to work around a bug.
        var children = InternalChildren;
        
        // Measure size of first child.
        var columns = ColumnCount(availableSize);
        return new Size(availableSize.Width, RowCount(children, columns) * ItemHeight);
    }

    private int ColumnCount(Size availableSize) => (int)(availableSize.Width / ColumnWidth);
    private int RowCount(UIElementCollection children, int columnCount) => (children.Count - 1) / columnCount + 1;
}