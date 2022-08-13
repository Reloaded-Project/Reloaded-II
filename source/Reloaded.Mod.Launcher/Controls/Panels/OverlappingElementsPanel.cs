using Size = System.Windows.Size;

namespace Reloaded.Mod.Launcher.Controls.Panels;

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