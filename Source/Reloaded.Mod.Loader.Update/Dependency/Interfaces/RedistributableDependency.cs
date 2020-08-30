namespace Reloaded.Mod.Loader.Update.Dependency.Interfaces
{
    public class RedistributableDependency : IDependency
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool Available { get; }
        public bool Is64Bit { get; }

        public RedistributableDependency(string name, bool available, bool is64Bit)
        {
            Available = available;
            Is64Bit  = is64Bit;
            Name = name;
        }

        /// <inheritdoc />
        public string[] GetUrls()
        {
            return Is64Bit
                ? new[]{ "https://aka.ms/vs/16/release/VC_redist.x64.exe" }
                : new[]{ "https://aka.ms/vs/16/release/VC_redist.x86.exe" };
        }
    }
}
