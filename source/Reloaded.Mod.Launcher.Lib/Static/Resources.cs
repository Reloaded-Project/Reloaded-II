#pragma warning disable CS8618
#pragma warning disable CS1591

namespace Reloaded.Mod.Launcher.Lib.Static;

/// <summary>
/// Contains a list of all expected resource keys available from a given <see cref="IDictionaryResourceProvider"/>.
/// </summary>
public static class Resources
{
    /// <summary>
    /// Provider allowing you to access individual dictionary resources.
    /// </summary>
    public static IDictionaryResourceProvider ResourceProvider { get; private set; }

    /// <summary>
    /// Initializes the resource set with a known provider.
    /// </summary>
    /// <param name="provider">Provider for the resources contained within.</param>
    public static void Init(IDictionaryResourceProvider provider)
    {
        ResourceProvider = provider;

        // Use reflection to populate every member.
        var thisType     = typeof(Resources);
        var providerType = provider.GetType();

        foreach (var property in thisType.GetProperties())
        {
            if (!property.PropertyType.IsGenericType || property.PropertyType.GetGenericTypeDefinition() != typeof(IDictionaryResource<>))
                continue;

            var setValueMethod = providerType.GetMethod(nameof(ResourceProvider.GetResource));
            var genericMethod = setValueMethod!.MakeGenericMethod(property.PropertyType.GenericTypeArguments[0] );
            var value = genericMethod.Invoke(ResourceProvider, new object[] { property.Name });
            property.SetValue(null, value);
        }
    }

    // NOTE: Property Names are the Keys!!

    // Splash Screen: Progress Messages
    public static IDictionaryResource<string> SplashCreatingDefaultConfig { get; set; }
    public static IDictionaryResource<string> SplashPreparingResources { get; set; }
    public static IDictionaryResource<string> SplashLoadCompleteIn { get; set; }
    public static IDictionaryResource<string> SplashRunningSanityChecks { get; set; }

    // Add Application Page
    public static IDictionaryResource<string> AddAppImageSelectorTitle { get; set; }
    public static IDictionaryResource<string> AddAppImageSelectorFilter { get; set; }

    public static IDictionaryResource<string> AddAppExecutableTitle { get; set; }
    public static IDictionaryResource<string> AddAppExecutableFilter { get; set; }

    public static IDictionaryResource<string> AddAppShortcutCreatedTitle { get; set; }
    public static IDictionaryResource<string> AddAppShortcutCreatedMessage { get; set; }

    public static IDictionaryResource<string> CreateModDialogImageSelectorTitle { get; set; }
    public static IDictionaryResource<string> CreateModDialogImageSelectorFilter { get; set; }

    public static IDictionaryResource<string> TitleCreateModNonUniqueId { get; set; }
    public static IDictionaryResource<string> MessageCreateModNonUniqueId { get; set; }

    // Application Entry
    public static IDictionaryResource<string> ApplicationOtherInstances { get; set; } // Unused as of 1.7.0

    // Application & SubMenus
    public static IDictionaryResource<string> LoadModSetDialogTitle { get; set; }
    public static IDictionaryResource<string> SaveModSetDialogTitle { get; set; }
    
    // Update Loader Dialog
    public static IDictionaryResource<string> UpdateLoaderChangelogUnavailable { get; set; }

    public static IDictionaryResource<string> UpdateLoaderRunningTitle { get; set; }
    public static IDictionaryResource<string> UpdateLoaderRunningMessage { get; set; }
    public static IDictionaryResource<string> UpdateLoaderProcessList { get; set; }

    // No Update Dialog
    public static IDictionaryResource<string> NoUpdateDialogTitle { get; set; }
    public static IDictionaryResource<string> NoUpdateDialogMessage { get; set; }

    // Fetch Nuget Dialog
    public static IDictionaryResource<string> FetchNugetNotFoundMessage { get; set; }
    public static IDictionaryResource<string> FetchNugetNotFoundAdvice { get; set; }
    
    // Errors
    public static IDictionaryResource<string> ErrorError { get; set; } // Just the word "Error" in native language.
    public static IDictionaryResource<string> ErrorUnknown { get; set; }
    public static IDictionaryResource<string> ErrorPathNullOrEmpty { get; set; }
    public static IDictionaryResource<string> ErrorFailedToGetDirectoryOfMod { get; set; }
    public static IDictionaryResource<string> ErrorFailedToGetDirectoryOfApplication { get; set; }
    public static IDictionaryResource<string> ErrorLoaderNotFound { get; set; } // Loader DLL Name displays before this message
    public static IDictionaryResource<string> ErrorFailedToStartProcess { get; set; }
    public static IDictionaryResource<string> ErrorDllInjectionFailed { get; set; }
    public static IDictionaryResource<string> ErrorPathToApplicationInvalid { get; set; }
    public static IDictionaryResource<string> ErrorCheckUpdatesFailed { get; set; }
    public static IDictionaryResource<string> ErrorGetProcAddress32Failed { get; set; }

    // Asi Loader
    public static IDictionaryResource<string> AsiLoaderDialogTitle { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogDescription { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogLoaderDeployed { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogBootstrapperDeployed { get; set; }

    // Update 1.8.0: Extra Error(s)
    public static IDictionaryResource<string> ErrorFailedToObtainPort { get; set; }

    // Update 1.10.0: Reloaded Bootstrapper Update
    public static IDictionaryResource<string> BootstrapperUpdateTitle { get; set; }
    public static IDictionaryResource<string> BootstrapperUpdateDescription { get; set; }

    // Update 1.10.1: Reloaded Bootstrapper Hotfix
    public static IDictionaryResource<string> BootstrapperCreateDirectoryError { get; set; }

    // Update 1.12.0: Additional Errors
    public static IDictionaryResource<string> ErrorUnableDetermineVersion { get; set; }
    public static IDictionaryResource<string> ErrorUpdateModInUseTitle { get; set; }
    public static IDictionaryResource<string> ErrorUpdateModInUse { get; set; }
    public static IDictionaryResource<string> ErrorMissingDependency { get; set; }
    public static IDictionaryResource<string> ErrorInvalidSemanticVersionTitle { get; set; }
    public static IDictionaryResource<string> ErrorInvalidSemanticVersionDescription { get; set; }

    // Update 1.12.0: Edit Application Ex 
    public static IDictionaryResource<string> AddAppWarningTitle { get; set; }
    public static IDictionaryResource<string> AddAppWarning { get; set; }
    
    // Update 1.12.0: Publish Mod Dialog
    public static IDictionaryResource<string> PublishSelectConfigTitle { get; set; }
    public static IDictionaryResource<string> PublishSelectConfigFileTypeName { get; set; }

    public static IDictionaryResource<string> ErrorInvalidModConfigTitle { get; set; }
    public static IDictionaryResource<string> ErrorInvalidModConfigDescription { get; set; }

    public static IDictionaryResource<string> ErrorNewerModConfigVersionTitle { get; set; }
    public static IDictionaryResource<string> ErrorNewerModConfigVersionDescription { get; set; }

    public static IDictionaryResource<string> ErrorPublishModIdMismatch { get; set; }
    public static IDictionaryResource<string> ErrorPublishModIdDescription { get; set; }
    
    public static IDictionaryResource<string> PublishModRegexDialogTitle { get; set; }

    public static IDictionaryResource<string> PublishAutoDeltaWarningTitle { get; set; }
    public static IDictionaryResource<string> PublishAutoDeltaWarningDescriptionFormat { get; set; }
    public static IDictionaryResource<string> PublishModWarningTitle { get; set; }
    public static IDictionaryResource<string> PublishModWarningDescription { get; set; }

    // Update 1.12.0: Edit Mod User Config

    // Update 1.12.0: Fast Package Download Experience
    public static IDictionaryResource<string> PackageDownloaderDownloadCompleteTitle { get; set; }
    public static IDictionaryResource<string> PackageDownloaderDownloadCompleteDescription { get; set; }
    public static IDictionaryResource<string> PackageDownloaderDownloadingDependencies { get; set; }

    // Update 1.12.0: Extended Download Mods Menu
    public static IDictionaryResource<string> DownloadPackagesAll { get; set; }
    

    // Update 1.12.0: Community App Repository
    public static IDictionaryResource<string> AddAppRepoBadExecutable { get; set; }

    public static IDictionaryResource<string> AddAppRepoTestJsonSelectTitle { get; set; }
    public static IDictionaryResource<string> AddAppRepoTestJsonSelectFilter { get; set; }

    // Update 1.20.0: Extended Publish Menu
    public static IDictionaryResource<string> PublishSelectMarkdownTitle { get; set; }
    public static IDictionaryResource<string> PublishSelectMarkdownTypeName { get; set; }

    // Update 1.20.0: Runtime Update
    public static IDictionaryResource<string> RuntimeUpdateRequiredTitle { get; set; }
    public static IDictionaryResource<string> RuntimeUpdateRequiredDescription { get; set; }

    // Update 1.21.0: Mod Packs
    public static IDictionaryResource<string> ModPackSelectExistingTitle { get; set; }
    public static IDictionaryResource<string> ModPackSelectExistingTypeName { get; set; }
    public static IDictionaryResource<string> ErrorFailedToSaveModPack { get; set; }

    // Update 1.21.0: Generic Image Selector
    public static IDictionaryResource<string> ImageSelectorTitle { get; set; }
    public static IDictionaryResource<string> ImageSelectorFilter { get; set; }

    // Update 1.21.0: Mod Packs Install
    public static IDictionaryResource<string> InstallModPackDownloading { get; set; }
    public static IDictionaryResource<string> InstallModPackErrorDownloadFail { get; set; }
    
    // Update 1.21.6: Mod Packs Install
    public static IDictionaryResource<string> ErrorAddApplicationGeneral { get; set; }
    public static IDictionaryResource<string> ErrorAddApplicationCantReadSymlink { get; set; }
    public static IDictionaryResource<string> ErrorAddApplicationGetVersionInfo { get; set; }
    public static IDictionaryResource<string> ErrorCantReadExeFile { get; set; }
    public static IDictionaryResource<string> ErrorCantReadExeFileAsiLoaderDeploy { get; set; }

    // Update 1.23.0: Search Options
    public static IDictionaryResource<string> SearchOptionSortNone { get; set; }
    public static IDictionaryResource<string> SearchOptionSortLastModified { get; set; }
    public static IDictionaryResource<string> SearchOptionSortDownloads { get; set; }
    public static IDictionaryResource<string> SearchOptionSortLikes { get; set; }
    public static IDictionaryResource<string> SearchOptionSortViews { get; set; }
    public static IDictionaryResource<string> SearchOptionAscending { get; set; }
    public static IDictionaryResource<string> SearchOptionDescending { get; set; }

    // Update 1.26.0: GamePass ASI Loader Auto Deploy
    public static IDictionaryResource<string> AsiLoaderGamePassAutoInstallFail { get; set; }
    
    // Update 1.26.0: Drag & Drop Mods
    public static IDictionaryResource<string> DragDropInstalledModsTitle { get; set; }
    public static IDictionaryResource<string> DragDropInstalledModsDescription { get; set; }
}