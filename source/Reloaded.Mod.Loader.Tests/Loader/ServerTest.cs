using System;
using System.Linq;
using System.Threading.Tasks;
using Reloaded.Mod.Loader.Mods.Structs;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Messages.Structures;
using Reloaded.Mod.Loader.Tests.SETUP;
using TestInterfaces;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Loader;

public class ServerTest : IDisposable
{
    private static Random _random = new Random();
    private Mod.Loader.Loader _loader;
    private TestEnvironmoent _testEnvironmoent;
    private Host _host;
    private Client _client;

    public ServerTest()
    {
        _testEnvironmoent = new TestEnvironmoent();

        _loader = new Mod.Loader.Loader(true);
        _loader.LoadForCurrentProcess();

        _host = new Host(_loader);
        _client = new Client(_host.Port);
    }

    public void Dispose()
    {
        _testEnvironmoent?.Dispose();
        _loader?.Dispose();
    }

    [Fact]
    public async Task LoadedModsCount()
    {
        var  response = await _client.GetLoadedModsAsync();

        int loadedMods = _loader.Manager.GetLoadedMods().Length;
        Assert.Equal(loadedMods, response.Mods.Length);
    }

    [Fact]
    public async Task AssertLoadedMods()
    {
        var response = await _client.GetLoadedModsAsync();

        ModInstance[]   localLoadedMods = _loader.Manager.GetLoadedMods();
        ModInfo[]       remoteLoadedMods = response.Mods;

        bool Equals(ModInstance instance, ModInfo info)
        {
            return instance.State == info.State &&
                   instance.ModConfig.ModId == info.ModId &&
                   instance.CanSuspend == info.CanSuspend &&
                   instance.CanUnload == info.CanUnload;
        }

        foreach (var remoteLoadedMod in remoteLoadedMods)
            Assert.Contains(localLoadedMods, instance => Equals(instance, remoteLoadedMod));
    }

    [Fact]
    public async Task LoadMod()
    { 
        await _client.LoadMod(_testEnvironmoent.TestModConfigC.ModId);

        // Should be loaded last.
        var loadedMods = _loader.Manager.GetLoadedMods();
        var testModC = (ITestHelper) loadedMods.First(x => x.ModConfig.ModId == _testEnvironmoent.TestModConfigC.ModId).Mod;
        Assert.Equal(_testEnvironmoent.TestModConfigC.ModId, testModC.MyId);

        // Check state consistency
        Assert.Equal(ModState.Running, loadedMods.First(x => x.ModConfig.ModId == _testEnvironmoent.TestModConfigC.ModId).State);
    }

    [Fact]
    public async Task LoadDuplicateMod()
    {
        bool testPassed = false;
        _client.OnReceiveException += response => { testPassed = true; };

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            // No Response
            await _client.LoadMod(_testEnvironmoent.TestModConfigA.ModId);
        });
            
        Assert.True(testPassed);
    }

    [Fact]
    public async Task UnloadMod()
    {
        await _client.UnloadModAsync(_testEnvironmoent.TestModConfigB.ModId);

        // Should be loaded last.
        var loadedMods        = _loader.Manager.GetLoadedMods();
        ModInstance configB   = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testEnvironmoent.TestModConfigB.ModId);

        Assert.Null(configB);
    }

    [Fact]
    public async Task SuspendMod()
    {
        await _client.SuspendModAsync(_testEnvironmoent.TestModConfigB.ModId);

        // Get instance for B
        var loadedMods = _loader.Manager.GetLoadedMods();
        ModInstance configB = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testEnvironmoent.TestModConfigB.ModId);

        Assert.Equal(ModState.Suspended, configB.State);
    }

    [Fact]
    public async Task SuspendAndResumeMod()
    {
        // Suspend first.
        await SuspendMod();

        // Now resume.
        await _client.ResumeModAsync(_testEnvironmoent.TestModConfigB.ModId);

        // Get instance for B
        var loadedMods      = _loader.Manager.GetLoadedMods();
        ModInstance configB = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testEnvironmoent.TestModConfigB.ModId);

        Assert.Equal(ModState.Running, configB.State);
    }
}