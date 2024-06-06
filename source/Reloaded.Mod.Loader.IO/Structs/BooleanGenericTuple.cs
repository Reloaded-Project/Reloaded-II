namespace Reloaded.Mod.Loader.IO.Structs;

public class BooleanGenericTuple<TGeneric> : ObservableObject
{
    public const string NameOfEnabled = nameof(Enabled);

    public bool Enabled { get; set; }
    public TGeneric Generic { get; set; }

    public BooleanGenericTuple(bool enabled, TGeneric generic)
    {
        Enabled = enabled;
        Generic = generic;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        // For Debugging and Screen Readers
        return $"Enabled: {Enabled}, Item: {Generic}";
    }
}