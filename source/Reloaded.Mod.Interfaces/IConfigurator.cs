namespace Reloaded.Mod.Interfaces;

/// <summary>
/// Alias for <see cref="IConfiguratorV1"/> for backwards compatibility reasons.
/// </summary>
[Obsolete("For backwards compatibility only. Use IConfiguratorV1, IConfiguratorV2 or desired versions.")]
public interface IConfigurator : IConfiguratorV1 { }