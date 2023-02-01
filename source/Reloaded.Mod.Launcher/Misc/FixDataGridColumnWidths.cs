namespace Reloaded.Mod.Launcher.Misc;

/// <summary>
/// Allows for automatic updating of Data Grid column widths where certain widths are set to Auto and certain widths have a star in place.
/// This fix requires that the NotifyOnTargetUpdated property is set on at least 1 column.
/// </summary>
public class FixDataGridColumnWidths : AttachedPropertyBase<FixDataGridColumnWidths, bool>
{
    /// <inheritdoc />
    public override void OnValueUpdated(DependencyObject sender, object value)
    {
        if (sender is DataGrid grid)
        {
            if ((bool)value == false) { DisableFix(grid); }
            else { EnableFix(grid); }
        }
    }

    private void DisableFix(DataGrid grid) => grid.TargetUpdated -= OnTargetUpdated;
    private void EnableFix(DataGrid grid)  => grid.TargetUpdated += OnTargetUpdated;

    private void OnTargetUpdated(object? sender, DataTransferEventArgs e)
    {
        if (!(sender is DataGrid grid)) 
            return;

        foreach (var column in grid.Columns)
        {
            if (!column.Width.IsStar) 
                continue;

            var oldWidth = column.Width;
            column.Width = 0;
            column.Width = oldWidth;
        }

        try { grid.UpdateLayout(); }
        catch (Exception) { }
    }
}