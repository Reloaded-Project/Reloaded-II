using Reloaded.Mod.Loader.Update.Packs;
using FileMode = System.IO.FileMode;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// Viewmodel used for editing Reloaded packs which abstract multiple downloadable mods.
/// </summary>
public class EditModPackDialogViewModel : ObservableObject
{
    /// <summary>
    /// The pack stored inside this viewmodel.
    /// </summary>
    public ObservablePack Pack { get; set; } = new();

    /// <summary>
    /// The currently selected mod item.
    /// </summary>
    public ObservablePackItem? SelectedItem { get; set; }

    /// <summary>
    /// Service providing access to all mods.
    /// </summary>
    public ModConfigService ModService { get; set; }

    /// <summary>
    /// The repository providing access to NuGet resources.
    /// </summary>
    public AggregateNugetRepository Repository { get; set; }

    /// <summary>
    /// Service that provides access to application configs
    /// </summary>
    public ApplicationConfigService ApplicationConfigService { get; set; }

    /// <summary>
    /// Creates a new ViewModel.
    /// </summary>
    /// <param name="repository">Provides access to NuGet sources. Optional.</param>
    /// <param name="applicationConfigService">Service that provides access to application configs.</param>
    /// <param name="modConfigService">Service that provides access to mod configs.</param>
    public EditModPackDialogViewModel(AggregateNugetRepository repository, ApplicationConfigService applicationConfigService, ModConfigService modConfigService)
    {
        Repository = repository;
        ApplicationConfigService = applicationConfigService;
        ModService = modConfigService;
    }

    /// <summary>
    /// Adds a mod to the underlying pack.
    /// </summary>
    /// <param name="selectedMod">The mod to add to the underlying pack.</param>
    public async Task<ObservablePackItem> AddModAsync(PathTuple<ModConfig> selectedMod)
    {
        var conf = selectedMod.Config;
        var build = new ReloadedPackBuilder();
        var providers = PackageProviderFactory.GetAllProviders(ApplicationConfigService.Items, Repository.Sources);
        var mod = await Task.Run(() => AutoPackCreator.CreateMod(build, conf, JxlImageConverter.Instance, providers, default)); // Magicscaler doesn't run on STA threads.

        // Extract the mod.
        var result = new ObservablePackItem(mod);
        Pack.Items.Add(result);
        return result;
    }

    /// <summary>
    /// Retrieves all mods eligible for selection in the mod pack.
    /// </summary>
    /// <returns>List of all mods that can be selected.</returns>
    public List<PathTuple<ModConfig>> GetEligibleModsForSelection()
    {
        var alreadyUsedIds = Pack.Items.Select(x => x.ModId).ToHashSet();
        return ModService.Items.Where(x => !alreadyUsedIds.Contains(x.Config.ModId) 
                                           && AutoPackCreator.ValidateCanCreate(x)).ToList();
    }

    /// <summary>
    /// Queries user for a save path and 
    /// </summary>
    public void SavePack()
    {
        try
        {
            var filePath = FileSelectors.SelectPackSaveFile();
            if (string.IsNullOrEmpty(filePath))
                return;

            var built = BuildPack(JxlImageConverter.Instance);
            using var file = File.Open(filePath, FileMode.Create);
            built.Position = 0;
            built.CopyTo(file);

        }
        catch (Exception e)
        {
            Errors.HandleException(e, Resources.ErrorFailedToSaveModPack.Get());
        }
    }

    /// <summary>
    /// Builds the pack stored behind this ViewModel.
    /// </summary>
    /// <returns>The built pack.</returns>
    public MemoryStream BuildPack(IModPackImageConverter converter)
    {
        var builder = new ReloadedPackBuilder();
        Pack.ToBuilder(builder, converter);
        var built = builder.Build(out _);
        return built;
    }

    /// <summary>
    /// Loads existing pack into memory.
    /// </summary>
    public void LoadExistingPack()
    {
        try
        {
            var filePath = FileSelectors.SelectPackFile();
            if (string.IsNullOrEmpty(filePath))
                return;

            using var fileStream = File.OpenRead(filePath);
            var reader = new ReloadedPackReader(fileStream);
            Pack = new ObservablePack(reader);
        }
        catch (Exception e)
        {
            Errors.HandleException(e);
        }
    }
}