namespace Reloaded.Mod.Loader.Tests.Update.Pack.Mocks;

public class DummyImageConverter : IImageConverter
{
    public MemoryStream Convert(Span<byte> source, out string extension)
    {
        extension = ".org";
        return new MemoryStream(source.ToArray());
    }
}