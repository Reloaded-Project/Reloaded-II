using System.Xml.Serialization;

namespace Reloaded.Mod.Loader.Update.Converters.NuGet.Structures
{
    public class DependencyGroup
    {
        [XmlArray(ElementName = "group")]
        [XmlArrayItem("dependency")]
        public Dependency[] Dependencies { get; set; }

        public DependencyGroup() { }
        public DependencyGroup(Dependency[] dependencies)
        {
            Dependencies = dependencies;
        }
    }
}
