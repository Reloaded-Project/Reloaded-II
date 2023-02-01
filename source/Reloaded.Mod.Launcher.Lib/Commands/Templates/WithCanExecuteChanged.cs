namespace Reloaded.Mod.Launcher.Lib.Commands.Templates;

/// <summary>
/// Base class that adds support for <see cref="CanExecuteChanged"/> to a class.
/// </summary>
public abstract class WithCanExecuteChanged
{
    /// <summary>
    /// The synchronization context to run all CanExecute commands on.
    /// </summary>
    public SynchronizationContext SynchronizationContext { get; set; } = Actions.SynchronizationContext;

    /// <summary/>
    protected void RaiseCanExecute(object sender, NotifyCollectionChangedEventArgs e) => SynchronizationContext.Post((x) => CanExecuteChanged!(sender, e), null);

    /// <summary>
    /// Default implementation of CanExecuteChanged.
    /// </summary>
    public event EventHandler? CanExecuteChanged = (sender, args) => { };
}