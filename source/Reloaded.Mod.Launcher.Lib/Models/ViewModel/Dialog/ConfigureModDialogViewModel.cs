namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Represents the ViewModel allowing for individual modifications to be configured.
/// </summary>
public partial class ConfigureModDialogViewModel : ObservableObject
{
    /// <summary>
    /// List of all configurable settings items.
    /// </summary>
    public IConfigurable[] Configurables { get; set; }

    /// <summary>
    /// Current settings item to configure.
    /// </summary>
    public IConfigurable? CurrentConfigurable { get; set; }

    /// <summary/>
    public ConfigureModDialogViewModel(IConfigurable[] configurables)
    {
        Configurables = configurables;
        if (Configurables.Length > 0)
            CurrentConfigurable = Configurables[0];

        // For configurations which support updating, update them immediately when the configs are changed.
        for (int x = 0; x < Configurables.Length; x++)
        {
            if (Configurables[x] is not IUpdatableConfigurable updatableConfigurable) 
                continue;

            var xCopy = x;
            updatableConfigurable.ConfigurationUpdated += configurable =>
            {
                // Some PropertyGrids like the XCEED ones have no way of getting index of item.
                // For now, we will switch if necessary in the case that the name matches.
                // I don't see anyone making multiple configs with same names, it would be counter intuitive.
                if (Configurables[xCopy].ConfigName == CurrentConfigurable!.ConfigName)
                    CurrentConfigurable = configurable;

                Configurables[xCopy] = configurable;
            };
        }
    }

    /// <summary>
    /// Saves all configurations handled by this ViewModel.
    /// </summary>
    public void Save()
    {
        foreach (var configurable in Configurables)
            configurable.Save();
    }
}