namespace Reloaded.Mod.Installer.DependencyInstaller;

#nullable enable
/// <summary>
/// A class that splits up the reporting of progress into multiple "slices"; such that
/// multiple operations can be reported using one progress.
/// </summary>
public class ProgressSlicer
{
    private readonly IProgress<double>? _output;
    private readonly Dictionary<int, double> _splitTotals;

    private int _splitCount;

    /// <summary/>
    /// <param name="output">The progress instance to output the progress to.</param>
    public ProgressSlicer(IProgress<double>? output)
    {
        _output = output;
        _splitTotals = new Dictionary<int, double>();
    }

    /// <summary>
    /// Creates a slice, allowing to report part of the full (1.0)
    /// progress to the configured output as a 0.0 to 1.0 value.
    /// </summary>
    /// <param name="multiplier">
    ///     The amount of percent this slice is worth.
    ///     A value between 0.0 and 1.0, with 1.0 representing 100% of the progress.
    /// </param>
    public IProgress<double> Slice(double multiplier)
    {
        var index = _splitCount++;
        return new Progress<double>(p =>
        {
            lock (_splitTotals)
            {
                _splitTotals[index] = multiplier * p;
                _output?.Report(_splitTotals.Values.Sum());
            }
        });
    }
}
#nullable disable