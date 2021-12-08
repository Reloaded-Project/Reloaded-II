using System.Xml.Serialization;
#pragma warning disable CS1591

namespace Reloaded.Mod.Loader.Update.Converters.NuGet.Structures;

public class Package
{
    [XmlElement(ElementName = "metadata")]
    public Metadata Metadata { get; set; }

    public Package() { }
    public Package(Metadata metadata)
    {
        Metadata = metadata;
    }
}