using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.Update.Dependency.Interfaces;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Dialog;

/// <summary>
/// Represents an individual missing dependency to be downloaded and/or installed.
/// </summary>
public class MissingDependency
{
    /// <summary>
    /// Name of the dependency in question.
    /// </summary>
    public string Name => Dependency.Name;

    /// <summary>
    /// Whether the dependency is missing or not.
    /// </summary>
    public bool IsMissing => !Dependency.Available;

    /// <summary>
    /// The dependency in question.
    /// </summary>
    public IDependency Dependency;

    /// <summary/>
    public MissingDependency(IDependency dependency)
    {
        Dependency = dependency;
    }

    /// <summary>
    /// Opens all download URLs for the dependency components.
    /// </summary>
    public async Task OpenUrlsAsync()
    {
        foreach (var url in await Dependency.GetUrlsAsync())
            ProcessExtensions.OpenFileWithDefaultProgram(url);
    }
}