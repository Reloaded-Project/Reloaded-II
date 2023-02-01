using Size = System.Windows.Size;

namespace Reloaded.Mod.Launcher.Controls.Panels;

/// <summary>
/// Panel that draws all controls in an overlapping fashion, like a Grid when no columns or rows are specified.
/// </summary>
public class OverlappingElementsPanel : Panel
{
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        foreach (UIElement child in InternalChildren)
            child.Arrange(new Rect(arrangeSize));

        return arrangeSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var maxWidth  = 0.0;
        var maxHeight = 0.0;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(availableSize);
            var desired = child.DesiredSize;
            if (desired.Width > maxWidth)
                maxWidth = desired.Width;

            if (desired.Height > maxHeight)
                maxHeight = desired.Height;
        }

        return new Size(maxWidth, maxHeight);
    }
}