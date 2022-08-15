namespace Reloaded.Mod.Loader.Tests.Loader;

public class LnlServerHost : IDisposable
{
    private static Random _random = new Random();
    private Mod.Loader.Loader _loader;
    private TestEnvironmoent _testEnvironment;
    private LiteNetLibServer _host;
    private LiteNetLibClient _client;
    private const int RequestTimeout = 20000;

    public LnlServerHost()
    {
        _testEnvironment = new TestEnvironmoent();

        _loader = new Mod.Loader.Loader(true);
        _loader.LoadForCurrentProcess();

        _host = LiteNetLibServer.Create(_loader.Logger, _loader.Manager.LoaderApi, new Utils.Server.Configuration.Config());
        _client = new LiteNetLibClient(IPAddress.Loopback, "", _host.Host.Manager.LocalPort);
    }

    public void Dispose()
    {
        _testEnvironment?.Dispose();
        _loader?.Dispose();
    }

    [Fact]
    public async Task LoadedModsCount()
    {
        var  response = await _client.GetLoadedModsAsync(RequestTimeout);

        int loadedMods = _loader.Manager.GetLoadedMods().Length;
        Assert.Equal(loadedMods, response.Second.Mods.Length);
    }

    [Fact]
    public async Task AssertLoadedMods()
    {
        var response = await _client.GetLoadedModsAsync();

        ModInstance[] localLoadedMods = _loader.Manager.GetLoadedMods();
        var remoteLoadedMods = response.Second.Mods;

        bool Equals(ModInstance instance, ServerModInfo info)
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
        await _client.LoadModAsync(_testEnvironment.TestModConfigC.ModId, RequestTimeout);

        // Should be loaded last.
        var loadedMods = _loader.Manager.GetLoadedMods();
        var testModC = (ITestHelper) loadedMods.First(x => x.ModConfig.ModId == _testEnvironment.TestModConfigC.ModId).Mod;
        Assert.Equal(_testEnvironment.TestModConfigC.ModId, testModC.MyId);

        // Check state consistency
        Assert.Equal(ModState.Running, loadedMods.First(x => x.ModConfig.ModId == _testEnvironment.TestModConfigC.ModId).State);
    }

    [Fact]
    public async Task LoadDuplicateMod()
    {
        bool receivedException = false;
        _client.OnReceiveException += response => { receivedException = true; };

        var loadDuplicate = await _client.LoadModAsync(_testEnvironment.TestModConfigA.ModId, RequestTimeout);
            
        Assert.True(loadDuplicate.IsFirst);
        Assert.True(loadDuplicate.First.IsException());
        Assert.True(receivedException);
    }

    [Fact]
    public async Task UnloadMod()
    {
        await _client.UnloadModAsync(_testEnvironment.TestModConfigB.ModId, RequestTimeout);

        // Should be loaded last.
        var loadedMods        = _loader.Manager.GetLoadedMods();
        ModInstance configB   = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testEnvironment.TestModConfigB.ModId);

        Assert.Null(configB);
    }

    [Fact]
    public async Task SuspendMod()
    {
        await _client.SuspendModAsync(_testEnvironment.TestModConfigB.ModId, RequestTimeout);

        // Get instance for B
        var loadedMods = _loader.Manager.GetLoadedMods();
        ModInstance configB = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testEnvironment.TestModConfigB.ModId);

        Assert.Equal(ModState.Suspended, configB.State);
    }

    [Fact]
    public async Task SuspendAndResumeMod()
    {
        // Suspend first.
        await SuspendMod();

        // Now resume.
        await _client.ResumeModAsync(_testEnvironment.TestModConfigB.ModId, RequestTimeout);

        // Get instance for B
        var loadedMods      = _loader.Manager.GetLoadedMods();
        ModInstance configB = loadedMods.FirstOrDefault(x => x.ModConfig.ModId == _testEnvironment.TestModConfigB.ModId);

        Assert.Equal(ModState.Running, configB.State);
    }
}