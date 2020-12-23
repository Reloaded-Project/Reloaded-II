using System.Xml.Serialization;

namespace Reloaded.Mod.Loader.Update.Converters.NuGet.Structures
{
    public class Metadata
    {
        [XmlElement(ElementName = "title")]
        public string Title    { get; set; }

        [XmlElement(ElementName = "id")]
        public string Id      { get; set; }

        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "authors")]
        public string Authors { get; set; }

        [XmlElement(ElementName = "description")]
        public string Description           { get; set; }

        [XmlElement(ElementName = "dependencies")]
        public DependencyGroup Dependencies { get; set; }

        public Metadata() { }
        public Metadata(string title, string id, string version, string authors, string description, DependencyGroup dependencies)
        {
            Title = title;
            Id = id;
            Version = version;
            Authors = authors;
            Description = description;
            Dependencies = dependencies;
        }
    }
}
