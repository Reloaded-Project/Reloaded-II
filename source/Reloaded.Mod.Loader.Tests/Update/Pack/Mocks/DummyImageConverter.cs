namespace Reloaded.Mod.Loader.Tests.Update.Pack.Mocks;

public class DummyImageConverter : IModPackImageConverter
{
    public MemoryStream Convert(Span<byte> source, string extension, out string newExtension)
    {
        newExtension = extension;
        return new MemoryStream(source.ToArray());
    }
}