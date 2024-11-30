#pragma warning disable CS1591

namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces;

public class RedistributableDependency : IDependency
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool Available { get; }

    /// <inheritdoc />
    public Architecture Architecture { get; }

    public RedistributableDependency(string name, bool available, Architecture architecture)
    {
        Available = available;
        Architecture = architecture;
        Name = name;
    }

    /// <inheritdoc />
    public Task<string[]> GetUrlsAsync()
    {
        switch (Architecture)
        {
            case Architecture.Amd64:
                return Task.FromResult(new []{ "https://aka.ms/vs/17/release/vc_redist.x64.exe" });
            case Architecture.x86:
                return Task.FromResult(new [] { "https://aka.ms/vs/17/release/vc_redist.x86.exe" });
            default:
                throw new NotSupportedException();
        }
    }
}