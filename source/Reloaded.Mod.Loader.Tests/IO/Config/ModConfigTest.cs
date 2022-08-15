namespace Reloaded.Mod.Loader.Tests.IO.Config;

public class ModConfigTest : IDisposable
{
    /* Setup Loader Config */
    public TestEnvironmoent TestEnvironmoent { get; set; } = new TestEnvironmoent();

    public void Dispose() => TestEnvironmoent?.Dispose();

    [Fact]
    public void GetNestedMissingDependencies()
    {
        var dependencies = ModConfig.GetDependencies(TestEnvironmoent.TestModConfigC);

        foreach (var missingDependency in TestEnvironmoent.NonexistingDependencies)
            Assert.Contains(missingDependency, dependencies.MissingConfigurations);
    }

    [Fact]
    public void GetNestedDependencies()
    {
        var dependencies = ModConfig.GetDependencies(TestEnvironmoent.TestModConfigC);

        Assert.Contains(TestEnvironmoent.TestModConfigA, dependencies.Configurations);
        Assert.Contains(TestEnvironmoent.TestModConfigB, dependencies.Configurations);
    }

    [Fact]
    public void SortMods()
    {
        var dependencies  = ModConfig.GetDependencies(TestEnvironmoent.TestModConfigC);
        var allMods       = new List<ModConfig>();

        allMods.Add(TestEnvironmoent.TestModConfigC);
        allMods.AddRange(dependencies.Configurations);
        allMods = ModConfig.SortMods(allMods);

        Assert.Equal(TestEnvironmoent.TestModConfigA, allMods[0]);
        Assert.Equal(TestEnvironmoent.TestModConfigB, allMods[1]);
        Assert.Equal(TestEnvironmoent.TestModConfigC, allMods[2]);
    }
}