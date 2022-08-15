namespace Reloaded.Mod.Loader.Tests.Index;

/// <summary>
/// Tests for serializing types I am uncertain about.
/// </summary>
public class SerializationTests : IndexTestCommon
{
    [Fact]
    public void NuGetVersion_CanSerialize_WhenNotNull()
    {
        var nugetVersion = new NuGetVersion(1,2,3,4);
        var text = JsonSerializer.Serialize(nugetVersion, Serializer.Options);
        var deserialized = JsonSerializer.Deserialize<NuGetVersion>(text, Serializer.Options);
        Assert.Equal(nugetVersion, deserialized);
    }

    [Fact]
    public void NuGetVersion_CanSerialize_WhenSemVer()
    {
        var nugetVersion = new NuGetVersion("1.2.3-sth");
        var text = JsonSerializer.Serialize(nugetVersion, Serializer.Options);
        var deserialized = JsonSerializer.Deserialize<NuGetVersion>(text, Serializer.Options);
        Assert.Equal(nugetVersion, deserialized);
    }

    [Fact]
    public void NuGetVersion_CanSerialize_WhenNull()
    {
        var tuple = new TestObject()
        {
            RandomValue = 0x696C6F7665796F75,
            Version = null
        };

        var text = JsonSerializer.Serialize(tuple, Serializer.Options);
        var deserialized = JsonSerializer.Deserialize<TestObject>(text, Serializer.Options);
        Assert.Equal(tuple, deserialized);
    }

    #region Dummy Object
    public class TestObject : IEquatable<TestObject>
    {
        public NuGetVersion Version { get; set; }
        public long RandomValue { get; set; }

        public bool Equals(TestObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Version, other.Version) && RandomValue == other.RandomValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestObject)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Version, RandomValue);
        }
    }
    #endregion
}