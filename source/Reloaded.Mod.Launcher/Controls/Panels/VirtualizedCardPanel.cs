using Point = System.Windows.Point;
using ScrollViewer = System.Windows.Controls.ScrollViewer;
using Size = System.Windows.Size;

namespace Reloaded.Mod.Launcher.Controls.Panels;

/// <summary>
/// Panel that accepts a number of equally sized children (using the first child as reference for size).
/// Stretches the results horizontally, and wraps around after a de
/// </summary>
public class VirtualizedCardPanel : VirtualizingPanel, IScrollInfo
{
    /// <summary/>
    public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register(nameof(ColumnWidth), typeof(double), typeof(VirtualizedCardPanel), new PropertyMetadata(default(double)));
    
    /// <summary/>
    public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
        nameof(ItemWidth), typeof(double), typeof(VirtualizedCardPanel), new PropertyMetadata(default(double)));

    /// <summary/>
    public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
        nameof(ItemHeight), typeof(double), typeof(VirtualizedCardPanel), new PropertyMetadata(default(double)));

    /// <summary/>
    public static readonly DependencyProperty CallMeasureOnChildrenProperty = DependencyProperty.Register(
        nameof(CallMeasureOnChildren), typeof(bool), typeof(VirtualizedCardPanel), new PropertyMetadata(default(bool)));

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

    /// <summary>
    /// Declares whether Measure should be called on child elements.
    /// Might be necessary for some entirely custom controls but usually can be skipped as we don't need this info.
    /// </summary>
    public bool CallMeasureOnChildren
    {
        get => (bool)GetValue(CallMeasureOnChildrenProperty);
        set => SetValue(CallMeasureOnChildrenProperty, value);
    }

    // Position child element, return actual used size.
    protected override Size ArrangeOverride(Size finalSize)
    {
        // necessary to work around a WPF bug.
        var internalChildren = InternalChildren;
        var childrenCount = internalChildren.Count;

        var columnCount   = ColumnCount(finalSize);
        var rowCount      = RowCount(childrenCount, columnCount);

        var elementWidth  = finalSize.Width / columnCount;
        var childSize     = new Size(elementWidth, ItemHeight);

        // Update Scrollviewer
        var generator = this.ItemContainerGenerator;
        CalculateScrollviewerInfo(finalSize, rowCount);
        EnsureNoOverscroll();

        for (int x = 0; x < childrenCount; x++)
        {
            var itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(x, 0));
            var row       = itemIndex / columnCount;
            var remainder = itemIndex % columnCount;

            // On some edge cases involving removing items, this can go out of bounds.
            var pos = new Point(remainder * childSize.Width, (row * childSize.Height) - VerticalOffset);
            internalChildren[x].Arrange(new Rect(pos, childSize));
        }

        return new Size(finalSize.Width, finalSize.Height);
    }

    // Measure size in layout required for child elements.
    protected override Size MeasureOverride(Size availableSize)
    {
        // necessary to work around a WPF bug.
        // otherwise GetChildren returns empty.
        _ = InternalChildren;

        var children    = GetChildren();
        var columnCount = ColumnCount(availableSize);
        var rowCount    = RowCount(children.Count, columnCount);
        var itemHeight = ItemHeight;
        var itemWidth  = ItemWidth;

        // Calculate scroll data [for internal scroll viewer]
        CalculateScrollviewerInfo(availableSize, rowCount, true);
        EnsureNoOverscroll();

        // Virtualization Shenanigans
        DetermineVisibleItems(columnCount, children.Count - 1, out var startVisible, out int endVisible);
        RealizeItems(startVisible, endVisible, children, itemWidth, itemHeight);

        // Measure size of first child.
        return new Size(availableSize.Width, availableSize.Height);
    }

    private int ColumnCount(Size availableSize) => (int)(availableSize.Width / ColumnWidth);
    private int RowCount(int childrenCount, int columnCount) => (childrenCount - 1) / columnCount + 1;

    #region Virtualization Shenanigans

    private ItemCollection GetChildren()
    {
        return ItemsControl.GetItemsOwner(this).Items;
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                break;
        }
    }

    private void RealizeItems(int startVisible, int endVisible, ItemCollection children, double itemWidth, double itemHeight)
    {
        if (children.Count == 0)
            return;

        // Create visible items.
        var measureChildren = CallMeasureOnChildren;
        var generator = ItemContainerGenerator;

        // Get the generator position of the first visible data item
        var startPos = generator.GeneratorPositionFromIndex(startVisible);

        // Get index where we'd insert the child for this position. If the item is realized
        // (startPos.Offset == 0), it's just startPos.Index, otherwise we have to add one to
        // insert after the corresponding child.
        // We do this because we generate next.
        var childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
        using var at = generator.StartAt(startPos, GeneratorDirection.Forward, true);
        for (int itemIndex = startVisible; itemIndex <= endVisible; ++itemIndex, ++childIndex)
        {
            // Get or create the child
            var child = generator.GenerateNext(out var newlyRealized) as UIElement;
            if (newlyRealized)
            {
                // Figure out if we need to insert the child at the end or somewhere in the middle
                if (childIndex >= children.Count)
                    AddInternalChild(child);
                else
                    InsertInternalChild(childIndex, child);

                generator.PrepareItemContainer(child);
            }

            // Measurements will depend on layout algorithm
            if (measureChildren)
                child!.Measure(new Size(itemWidth, itemHeight));
        }

        CleanUpItems(startVisible, endVisible);
    }

    private void CleanUpItems(int firstVisibleItemIndex, int lastVisibleItemIndex)
    {
        var children = InternalChildren;
        var generator = ItemContainerGenerator;

        for (int x = children.Count - 1; x >= 0; x--)
        {
            // Map a child index to an item index by going through a generator position
            var childGeneratorPos = new GeneratorPosition(x, 0);
            int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);

            if (itemIndex < firstVisibleItemIndex || itemIndex > lastVisibleItemIndex)
            {
                generator.Remove(childGeneratorPos, 1);
                RemoveInternalChildRange(x, 1);
            }
        }
    }


    // Determine Visible Items
    private void DetermineVisibleItems(int columnCount, int maxItemIndex, out int startVisible, out int endVisible)
    {
        // Determine items that overlap.
        var viewHeight = ViewportHeight;
        var viewOffset = VerticalOffset;
        var itemHeight = ItemHeight;

        // Calculate first visible row.
        var firstVisibleRow = (int)(viewOffset / itemHeight);
        startVisible = (firstVisibleRow) * columnCount;
        if (startVisible < 0)
            startVisible = 0;

        // Calculate last visible row.
        var lastVisibleRow = (int)((viewOffset + viewHeight) / itemHeight);

        // we must go to next row, because last columnCount items are visible!
        endVisible = ((lastVisibleRow + 1) * (columnCount) - 1); // -1 for max index rather than max count.
        if (endVisible > maxItemIndex)
            endVisible = maxItemIndex;
    }
    
    #endregion


    #region IScrollInfo
    // https://docs.microsoft.com/en-us/archive/blogs/bencon/iscrollinfo-in-avalon-part-i
    private const double LineSize = 32;
    private const double WheelSize = 3 * LineSize;

    public bool CanHorizontallyScroll { get; set; } = false;
    public bool CanVerticallyScroll { get; set; } = true;

    // Total size of panel
    public double ExtentHeight { get; internal set; }
    public double ExtentWidth { get; internal set; }
    
    // Scroll offset
    public double HorizontalOffset { get; internal set; }
    public double VerticalOffset { get; internal set; }

    // This one is assigned by the scrollviewer above us in visual tree! when it sees this interface
    public ScrollViewer? ScrollOwner { get; set; } = null!;

    // The size we can see
    public double ViewportHeight { get; internal set; }
    public double ViewportWidth { get; internal set; }

    public void LineDown() => SetVerticalOffset(VerticalOffset + LineSize);

    public void LineUp() => SetVerticalOffset(VerticalOffset - LineSize);

    public void LineLeft() => SetHorizontalOffset(HorizontalOffset - LineSize);

    public void LineRight() => SetHorizontalOffset(HorizontalOffset + LineSize);

    public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + WheelSize);

    public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - WheelSize);

    public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - WheelSize);

    public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + WheelSize);

    public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);

    public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);

    public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);

    public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);

    private bool _dirtyScrollInfo;

    /// <summary>
    /// Forces an item defined by visual to be visible.
    /// </summary>
    /// <param name="visual">A <see cref="Visual"/> that becomes visible.</param>
    /// <param name="rectangle">The rectangle to make visible.</param>
    /// <returns></returns>
    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        for (int x = 0; x < InternalChildren.Count; x++)
        {
            if (InternalChildren[x] != visual) 
                continue;

            // Found the visual! Let's scroll it into view.
            var columnCount = ColumnCount(RenderSize);
            var position = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(x, 0));

            var row = position / columnCount;
            var itemHeight = ItemHeight;
            SetVerticalOffset((itemHeight * row) - (itemHeight / 4));
            return rectangle;
        }

        throw new ArgumentException("Given visual is not in this Panel");
    }

    public void SetHorizontalOffset(double offset)
    {
        // not supported
    }

    public void SetVerticalOffset(double offset)
    {
        // Make sure we don't overscroll
        offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
        if (VerticalOffset != offset)
        {
            VerticalOffset = offset;
            InvalidateMeasure();
            InvalidateArrange();
            _dirtyScrollInfo = true;
        }
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    private void CalculateScrollviewerInfo(Size availableSize, int rowCount, bool setExtent = false)
    {
        bool invalidateScrollInfo = false;

        if (ViewportWidth != availableSize.Width || ViewportHeight != availableSize.Height)
        {
            ViewportHeight = availableSize.Height;
            ViewportWidth = availableSize.Width;
            invalidateScrollInfo = true;
        }

        // Ensure not scrolled past end.
        if (setExtent)
        {
            var extentHeight = rowCount * ItemHeight;
            var extentWidth = availableSize.Width; 
            
            if (ExtentHeight != extentHeight || ExtentWidth != extentWidth)
            {
                ExtentHeight = extentHeight;
                ExtentWidth = extentWidth;
                invalidateScrollInfo = true;
            }
        }

        if (invalidateScrollInfo || _dirtyScrollInfo)
        {
            ScrollOwner?.InvalidateScrollInfo();
            _dirtyScrollInfo = false;
        }
    }

    private void EnsureNoOverscroll()
    {
        var verticalOffset = VerticalOffset;
        var offset = Math.Max(0, Math.Min(verticalOffset, ExtentHeight - ViewportHeight));
        if (verticalOffset != offset)
        {
            VerticalOffset = offset;
            _dirtyScrollInfo = true;
        }
    }
    #endregion
}