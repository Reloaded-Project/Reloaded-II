namespace Reloaded.Mod.Launcher.Controls.Properties;

/// <summary>
/// An attached WPF property that auto adjusts column count of a uniform grid based on a min width.
/// </summary>
public class AutoAdjustColumnCount : WPF.MVVM.AttachedPropertyBase<AutoAdjustColumnCount, int>
{
    public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
            return;
        
        // If attached to tooltip, handle via parent.
        if (sender is UniformGrid uniformGrid)
        {
            // Remove if already present, then re-add.
            uniformGrid.SizeChanged -= OnSizeChanged;
            uniformGrid.SizeChanged += OnSizeChanged;
            OnSizeChanged(sender, null!);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var grid     = (UniformGrid)sender;
        var minWidth = GetValue(grid);
        var numColumns = (int)grid.RenderSize.Width / minWidth;
        if (numColumns != grid.Columns)
            grid.Columns = numColumns;
    }
}