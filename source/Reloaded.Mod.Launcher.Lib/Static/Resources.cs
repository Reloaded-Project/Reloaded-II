using System;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Lib.Interop;
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

    // Page Titles
    public static IDictionaryResource<string> TitleAddApplication { get; set; }
    public static IDictionaryResource<string> TitleEditApplication { get; set; }
    public static IDictionaryResource<string> TitleManageMods { get; set; }
    public static IDictionaryResource<string> TitleDownloadMods { get; set; }
    public static IDictionaryResource<string> TitleLoaderSettings { get; set; }
    public static IDictionaryResource<string> TitleMainPage { get; set; }
    public static IDictionaryResource<string> TitleSearchApplications { get; set; } // Sidebar button to search applications. Removed. Might be brought back someday.
    public static IDictionaryResource<string> TitleSplashPage { get; set; }

    // Splash Screen: Progress Messages
    public static IDictionaryResource<string> SplashCreatingDefaultConfig { get; set; }
    public static IDictionaryResource<string> SplashCleaningConfigurations { get; set; }
    public static IDictionaryResource<string> SplashPreparingResources { get; set; }
    public static IDictionaryResource<string> SplashLoadCompleteIn { get; set; }
    public static IDictionaryResource<string> SplashCreatingTemplates { get; set; } // Unused as of 1.8.0
    public static IDictionaryResource<string> SplashCheckingForUpdates { get; set; }
    public static IDictionaryResource<string> SplashRunningSanityChecks { get; set; }

    // Add Application Page
    public static IDictionaryResource<string> AddAppImageTooltip { get; set; }
    public static IDictionaryResource<string> AddAppImageSelectorTitle { get; set; }
    public static IDictionaryResource<string> AddAppImageSelectorFilter { get; set; }

    public static IDictionaryResource<string> AddAppNewButton { get; set; }
    public static IDictionaryResource<string> AddAppDeleteButton { get; set; }

    public static IDictionaryResource<string> AddAppName { get; set; }
    public static IDictionaryResource<string> AddAppLocation { get; set; }
    public static IDictionaryResource<string> AddAppArguments { get; set; }

    public static IDictionaryResource<string> AddAppAdvancedOptions { get; set; }
    public static IDictionaryResource<string> AddAppCreateShortcut { get; set; }
    public static IDictionaryResource<string> AddAppAutoInject { get; set; }
    public static IDictionaryResource<string> AddAppAutoInjectMessage { get; set; }

    public static IDictionaryResource<string> AddAppExecutableTitle { get; set; }
    public static IDictionaryResource<string> AddAppExecutableFilter { get; set; }

    public static IDictionaryResource<string> AddAppShortcutCreatedTitle { get; set; }
    public static IDictionaryResource<string> AddAppShortcutCreatedMessage { get; set; }

    // Mod Manager Page
    public static IDictionaryResource<string> ModManagerSearchMods { get; set; }
    public static IDictionaryResource<string> ModManagerSearchApps { get; set; }
    public static IDictionaryResource<string> ModManagerExtraOptions { get; set; }
    public static IDictionaryResource<string> ModManagerOpenModDirectory { get; set; }
    public static IDictionaryResource<string> ModManagerConvertToNuget { get; set; }
    public static IDictionaryResource<string> ModManagerManageDependencies { get; set; }
    public static IDictionaryResource<string> ModManagerSelectMod { get; set; }
    public static IDictionaryResource<string> ModManagerSelectApp { get; set; }

    // Mod Manager: Set Dependencies
    public static IDictionaryResource<string> TitleSetDependenciesDialog { get; set; }

    // Mod Manager: Create Mod Page | Edit Mod Page since 1.12.0
    public static IDictionaryResource<string> TitleCreateModDialog { get; set; }
    public static IDictionaryResource<string> CreateModDialogSave { get; set; }
    public static IDictionaryResource<string> CreateModDialogId { get; set; }
    public static IDictionaryResource<string> CreateModDialogName { get; set; }
    public static IDictionaryResource<string> CreateModDialogAuthor { get; set; }
    public static IDictionaryResource<string> CreateModDialogVersion { get; set; }
    public static IDictionaryResource<string> CreateModDialogDescription { get; set; }
    public static IDictionaryResource<string> CreateModDialogDependencies { get; set; }

    public static IDictionaryResource<string> CreateModDialogImageSelectorTitle { get; set; }
    public static IDictionaryResource<string> CreateModDialogImageSelectorFilter { get; set; }

    public static IDictionaryResource<string> TitleCreateModNonUniqueId { get; set; }
    public static IDictionaryResource<string> MessageCreateModNonUniqueId { get; set; }

    // Mod Loader Settings
    public static IDictionaryResource<string> LoaderSettingsCleanup { get; set; }
    public static IDictionaryResource<string> LoaderSettingsCleanupWarning { get; set; }
    public static IDictionaryResource<string> LoaderSettingsCleanupComplete { get; set; }

    public static IDictionaryResource<string> LoaderSettingsAppsInstalled { get; set; }
    public static IDictionaryResource<string> LoaderSettingsModsInstalled { get; set; }
    public static IDictionaryResource<string> LoaderSettingsCompileDate { get; set; }
    public static IDictionaryResource<string> LoaderSettingsAndDocumentation { get; set; }
    public static IDictionaryResource<string> LoaderSettingsCopyright { get; set; }
    public static IDictionaryResource<string> LoaderSettingsEnableAutomaticUpdates { get; set; }
    public static IDictionaryResource<string> LoaderSettingsShowConsole { get; set; }
    public static IDictionaryResource<string> LoaderSettingsLoadAsynchronously { get; set; }
    public static IDictionaryResource<string> LoaderSettingsWarning { get; set; }

    // Application Entry
    public static IDictionaryResource<string> TitleApplication { get; set; }
    public static IDictionaryResource<string> ApplicationTotalMods { get; set; }
    public static IDictionaryResource<string> ApplicationOtherInstances { get; set; }       // Unused as of 1.7.0
    public static IDictionaryResource<string> ApplicationReloadedInstances { get; set; } // Unused as of 1.7.0

    // First Launch | Unused, Superseeded by 1.12.0 - New First Launch Experience
    public static IDictionaryResource<string> TitleFirstLaunch { get; set; } // Unused as of 1.12.0
    public static IDictionaryResource<string> FirstLaunchLineOne { get; set; }
    public static IDictionaryResource<string> FirstLaunchLineTwo { get; set; }
    public static IDictionaryResource<string> FirstLaunchLineDocumentation { get; set; }
    public static IDictionaryResource<string> FirstLaunchLineUserguide { get; set; }
    public static IDictionaryResource<string> FirstLaunchLineThree { get; set; }

    // Dialogs
    public static IDictionaryResource<string> MessageBoxButtonOK { get; set; }
    public static IDictionaryResource<string> MessageBoxButtonCancel { get; set; }

    // Non-Reloaded Process
    public static IDictionaryResource<string> TitleNonReloadedProcess { get; set; }
    public static IDictionaryResource<string> NonReloadedProcessMessageOne { get; set; }
    public static IDictionaryResource<string> NonReloadedProcessMessageTwo { get; set; }
    public static IDictionaryResource<string> NonReloadedProcessInject { get; set; }
    public static IDictionaryResource<string> NonReloadedProcessInjectWarning { get; set; }

    // Reloaded Process
    public static IDictionaryResource<string> TitleReloadedProcess { get; set; }

    // Application & SubMenus
    public static IDictionaryResource<string> TitleConfigureMods { get; set; }
    public static IDictionaryResource<string> ApplicationLaunch { get; set; }

    public static IDictionaryResource<string> ConfigureModsListItemTooltip { get; set; }
    public static IDictionaryResource<string> ConfigureModsDummyText { get; set; } // Unused for now, will be used down the road.
    public static IDictionaryResource<string> ConfigureModsConfigure { get; set; }
    public static IDictionaryResource<string> ConfigureModsOpenFolder { get; set; }

    public static IDictionaryResource<string> LoadModSet { get; set; }
    public static IDictionaryResource<string> SaveModSet { get; set; }
    public static IDictionaryResource<string> LoadModSetDialogTitle { get; set; }
    public static IDictionaryResource<string> SaveModSetDialogTitle { get; set; }

    public static IDictionaryResource<string> LoadModSetIncompatibleTitle { get; set; }
    public static IDictionaryResource<string> LoadModSetIncompatibleMessage { get; set; }
    public static IDictionaryResource<string> LoadModSetIncompatibleMessage2 { get; set; }

    // Reloaded Process
    public static IDictionaryResource<string> ReloadedProcessResume { get; set; }
    public static IDictionaryResource<string> ReloadedProcessSuspend { get; set; }
    public static IDictionaryResource<string> ReloadedProcessUnload { get; set; }
    public static IDictionaryResource<string> ReloadedProcessRefresh { get; set; }
    public static IDictionaryResource<string> ReloadedProcessLoadMod { get; set; }
    public static IDictionaryResource<string> ReloadedProcessNotice { get; set; }

    // Update Mod Dialog
    public static IDictionaryResource<string> UpdateModTitle { get; set; }
    public static IDictionaryResource<string> UpdateModDownload { get; set; }
    public static IDictionaryResource<string> UpdateModModId { get; set; }
    public static IDictionaryResource<string> UpdateModSize { get; set; }
    public static IDictionaryResource<string> UpdateModOldVersion { get; set; }
    public static IDictionaryResource<string> UpdateModNewVersion { get; set; }

    // Update Mod Confirmation Dialog
    public static IDictionaryResource<string> UpdateModConfirmTitle { get; set; }
    public static IDictionaryResource<string> UpdateModConfirmMessage { get; set; }

    // Update Loader Dialog
    public static IDictionaryResource<string> UpdateLoaderTitle { get; set; }
    public static IDictionaryResource<string> UpdateLoaderCurrentVersion { get; set; }
    public static IDictionaryResource<string> UpdateLoaderNewVersion { get; set; }

    public static IDictionaryResource<string> UpdateLoaderChangelogUnavailable { get; set; }
    public static IDictionaryResource<string> UpdateLoaderChangelog { get; set; }
    public static IDictionaryResource<string> UpdateLoaderUpdate { get; set; }

    public static IDictionaryResource<string> UpdateLoaderRunningTitle { get; set; }
    public static IDictionaryResource<string> UpdateLoaderRunningMessage { get; set; }
    public static IDictionaryResource<string> UpdateLoaderProcessList { get; set; }

    // Download Mods Page
    public static IDictionaryResource<string> DownloadModsDownload { get; set; }
    public static IDictionaryResource<string> DownloadModsDownloading { get; set; }
    public static IDictionaryResource<string> DownloadModsAlreadyDownloaded { get; set; }

    public static IDictionaryResource<string> DownloadModsVisitWebsite { get; set; }
    public static IDictionaryResource<string> DownloadModsCheckUpdatesAndDependencies { get; set; }

    // No Update Dialog
    public static IDictionaryResource<string> NoUpdateDialogTitle { get; set; }
    public static IDictionaryResource<string> NoUpdateDialogMessage { get; set; }

    // Fetch Nuget Dialog
    public static IDictionaryResource<string> FetchNugetTitle { get; set; }
    public static IDictionaryResource<string> FetchNugetMessage { get; set; }
    public static IDictionaryResource<string> FetchNugetNotFoundMessage { get; set; }
    public static IDictionaryResource<string> FetchNugetNotFoundAdvice { get; set; }

    // Startup Failure
    public static IDictionaryResource<string> FailedToLaunchTitle { get; set; }
    public static IDictionaryResource<string> FailedToLaunchMessage { get; set; }

    // Download Mod Archive
    public static IDictionaryResource<string> DownloadModArchiveTitle { get; set; }
    public static IDictionaryResource<string> DownloadModArchiveName { get; set; }

    // WMI Administrator Message
    public static IDictionaryResource<string> RunAsAdminMessage { get; set; }

    // Configure Mod Dialog
    public static IDictionaryResource<string> ConfigureModDialogTitle { get; set; }
    public static IDictionaryResource<string> ConfigureModDialogSave { get; set; }

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
    public static IDictionaryResource<string> AsiLoaderInstall { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogTitle { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogDescription { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogLoaderDeployed { get; set; }
    public static IDictionaryResource<string> AsiLoaderDialogBootstrapperDeployed { get; set; }
    public static IDictionaryResource<string> ErrorAsiNotSupported { get; set; } // Unused at the moment, may be used in the future.

    // Missing Dependencies Dialog
    public static IDictionaryResource<string> MissingDependenciesTitle { get; set; }
    public static IDictionaryResource<string> MissingDependenciesText { get; set; }
    public static IDictionaryResource<string> MissingDependenciesText2 { get; set; }

    // Categories: Main Menu Screen
    public static IDictionaryResource<string> AppCategoryMain { get; set; }
    public static IDictionaryResource<string> AppCategoryActions { get; set; }
    public static IDictionaryResource<string> AppCategoryProcesses { get; set; }
    public static IDictionaryResource<string> AppCategorySummary { get; set; }

    // Update 1.7.0: Loader Settings Icons
    public static IDictionaryResource<string> LoaderSettingsTwitter { get; set; }
    public static IDictionaryResource<string> LoaderSettingsDonate { get; set; }
    public static IDictionaryResource<string> LoaderSettingsDiscord { get; set; }

    // Update 1.7.0: NuGet Feeds
    public static IDictionaryResource<string> TitleConfigNuGetFeeds { get; set; }
    public static IDictionaryResource<string> ConfigNuGetFeedsButton { get; set; } // Button in Download Mods Menu

    public static IDictionaryResource<string> ConfigNuGetFeedsName { get; set; }
    public static IDictionaryResource<string> ConfigNuGetFeedsURL { get; set; }
    public static IDictionaryResource<string> ConfigNuGetFeedsURLTooltip { get; set; }
    public static IDictionaryResource<string> ConfigNuGetFeedsDescription { get; set; }

    // Update 1.7.9: Wine Compatibility Notes
    public static IDictionaryResource<string> WineCompatibilityNoticeTitle { get; set; }
    public static IDictionaryResource<string> WineCompatibilityNoticeText { get; set; }
    public static IDictionaryResource<string> WineCompatibilityNoticeOk { get; set; }

    // Update 1.8.0: Configuration
    public static IDictionaryResource<string> LoaderSettingsConfigPopup { get; set; }
    public static IDictionaryResource<string> LoaderSettingsConfigLanguage { get; set; }
    public static IDictionaryResource<string> LoaderSettingsConfigTheme { get; set; }
    public static IDictionaryResource<string> LoaderSettingsConfigLog { get; set; }
    public static IDictionaryResource<string> LoaderSettingsConfigFile { get; set; }

    // Update 1.8.0: Extra Error(s)
    public static IDictionaryResource<string> ErrorFailedToObtainPort { get; set; }

    // Update 1.10.0: Reloaded Bootstrapper Update
    public static IDictionaryResource<string> BootstrapperUpdateTitle { get; set; }
    public static IDictionaryResource<string> BootstrapperUpdateDescription { get; set; }

    // Update 1.10.1: Reloaded Bootstrapper Hotfix
    public static IDictionaryResource<string> BootstrapperCreateDirectoryError { get; set; }

    // Update 1.12.0: New First Launch Experience
    public static IDictionaryResource<string> TitleFirstLaunchEx { get; set; }

    public static IDictionaryResource<string> FirstLaunchExStepAddApplication { get; set; }
    public static IDictionaryResource<string> FirstLaunchExStepExtractMod { get; set; }
    public static IDictionaryResource<string> FirstLaunchExStepConfigureMod { get; set; }
    public static IDictionaryResource<string> FirstLaunchExStepEnableMod { get; set; }
    public static IDictionaryResource<string> FirstLaunchExStepComplete { get; set; }

    public static IDictionaryResource<string> FirstLaunchExAddApplicationButton { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddApplicationSkipTutorialButton { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddApplicationSkipStepButton { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddApplicationDescription1 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddApplicationDescription2 { get; set; }

    public static IDictionaryResource<string> FirstLaunchExAddModExtractDescription1 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddModExtractDescription2 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddModExtractPrev { get; set; }
    public static IDictionaryResource<string> FirstLaunchExAddModExtractNext { get; set; }

    public static IDictionaryResource<string> FirstLaunchExConfigureModDescription1 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExConfigureModDescription2 { get; set; }

    public static IDictionaryResource<string> FirstLaunchExEnableModDescription1 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExEnableModDescription2 { get; set; }

    public static IDictionaryResource<string> FirstLaunchExCompleteDescription1 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExCompleteDescription2 { get; set; }
    public static IDictionaryResource<string> FirstLaunchExCompleteDescription3 { get; set; }

    // Update 1.12.0: Context Menu for Application Mod Items
    public static IDictionaryResource<string> ModManagerContextPublish { get; set; }
    public static IDictionaryResource<string> ModManagerContextEdit { get; set; }
    public static IDictionaryResource<string> ModManagerContextEditMod { get; set; }
    public static IDictionaryResource<string> ModManagerContextEditModUser { get; set; }
    public static IDictionaryResource<string> ModManagerContextOpenFolder { get; set; }
    public static IDictionaryResource<string> ModManagerContextConfigure { get; set; }

    // Update 1.12.0: Edit Mod Dialog
    public static IDictionaryResource<string> TitleEditModDialog { get; set; }

    // Update 1.12.0: Additional Errors
    public static IDictionaryResource<string> ErrorUnableDetermineVersion { get; set; }
    public static IDictionaryResource<string> ErrorUpdateModInUseTitle { get; set; }
    public static IDictionaryResource<string> ErrorUpdateModInUse { get; set; }
    public static IDictionaryResource<string> ErrorMissingDependency { get; set; }
    public static IDictionaryResource<string> ErrorInvalidSemanticVersionTitle { get; set; }
    public static IDictionaryResource<string> ErrorInvalidSemanticVersionDescription { get; set; }

    // Update 1.12.0: Extended Edit Mod
    public static IDictionaryResource<string> CreateModDialogDescriptionShort { get; set; }
    public static IDictionaryResource<string> EditModStepMain { get; set; }
    public static IDictionaryResource<string> EditModStepDependencies { get; set; }
    public static IDictionaryResource<string> EditModStepUpdates { get; set; }
    public static IDictionaryResource<string> EditModStepComplete { get; set; }

    public static IDictionaryResource<string> EditModDependenciesDescription { get; set; }

    public static IDictionaryResource<string> EditModUniversalMod { get; set; }
    public static IDictionaryResource<string> EditModUniversalModDescription { get; set; }

    public static IDictionaryResource<string> EditModLibrary { get; set; }
    public static IDictionaryResource<string> EditModLibraryDescription { get; set; }
    public static IDictionaryResource<string> EditModSave { get; set; }

    public static IDictionaryResource<string> EditModAppsDescription { get; set; }
    public static IDictionaryResource<string> EditModMetadataNameTooltip { get; set; }

    // Update 1.12.0: Extended Application Configuration

    public static IDictionaryResource<string> ConfigureModsRightClickAdvice { get; set; }

    // Update 1.12.0: Create Mod Dialog 
    public static IDictionaryResource<string> CreateModDialogIdDesc1 { get; set; }
    public static IDictionaryResource<string> CreateModDialogIdDesc2 { get; set; }

    // Update 1.12.0: Edit Application Ex 
    public static IDictionaryResource<string> AddAppId { get; set; }
    public static IDictionaryResource<string> AddAppUpdate { get; set; }
    public static IDictionaryResource<string> AddAppWarningTitle { get; set; }
    public static IDictionaryResource<string> AddAppWarning { get; set; }
    
    // Update 1.12.0: Publish Mod Dialog
    public static IDictionaryResource<string> PublishModTitle { get; set; }
    public static IDictionaryResource<string> PublishModTarget { get; set; }
    public static IDictionaryResource<string> PublishModTargetTooltip { get; set; }
    public static IDictionaryResource<string> PublishModExcludeTitle { get; set; }
    public static IDictionaryResource<string> PublishModIncludeTitle { get; set; }
    public static IDictionaryResource<string> PublishModPreviousVersionsTitle { get; set; }
    public static IDictionaryResource<string> PublishModPreviousVersionsDescription { get; set; }
    public static IDictionaryResource<string> PublishModButtonText { get; set; }

    public static IDictionaryResource<string> PublishSelectConfigTitle { get; set; }
    public static IDictionaryResource<string> PublishSelectConfigFileTypeName { get; set; }

    public static IDictionaryResource<string> ErrorInvalidModConfigTitle { get; set; }
    public static IDictionaryResource<string> ErrorInvalidModConfigDescription { get; set; }

    public static IDictionaryResource<string> ErrorNewerModConfigVersionTitle { get; set; }
    public static IDictionaryResource<string> ErrorNewerModConfigVersionDescription { get; set; }

    public static IDictionaryResource<string> ErrorPublishModIdMismatch { get; set; }
    public static IDictionaryResource<string> ErrorPublishModIdDescription { get; set; }

    public static IDictionaryResource<string> PublishModRegexSectionTitle { get; set; }
    public static IDictionaryResource<string> PublishModRegexTitle { get; set; }
    public static IDictionaryResource<string> PublishModRegexTest { get; set; }
    public static IDictionaryResource<string> PublishModRegexDialogTitle { get; set; }

    public static IDictionaryResource<string> PublishModCompSettings { get; set; }
    public static IDictionaryResource<string> PublishModCompLevel { get; set; }
    public static IDictionaryResource<string> PublishModCompMethod { get; set; }

    public static IDictionaryResource<string> PublishOutputFolder { get; set; }
    public static IDictionaryResource<string> PublishSetOutputFolder { get; set; }

    public static IDictionaryResource<string> PublishAutoDelta { get; set; }
    public static IDictionaryResource<string> PublishAutoDeltaDescription { get; set; }

    public static IDictionaryResource<string> PublishAutoDeltaWarningTitle { get; set; }
    public static IDictionaryResource<string> PublishAutoDeltaWarningDescriptionFormat { get; set; }

    public static IDictionaryResource<string> PublishFileNameTooltip { get; set; }

    // Update 1.12.0: Edit Mod User Config
    public static IDictionaryResource<string> TitleEditModUserConfig { get; set; }
    public static IDictionaryResource<string> TitleEditModUserAllowPrereleases { get; set; }
    public static IDictionaryResource<string> TitleEditModUserAllowPrereleasesTooltip { get; set; }

    // Update 1.12.0: Fast Package Download Experience
    public static IDictionaryResource<string> PackageDownloaderDownloadCompleteTitle { get; set; }
    public static IDictionaryResource<string> PackageDownloaderDownloadCompleteDescription { get; set; }
    public static IDictionaryResource<string> PackageDownloaderDownloadingDependencies { get; set; }

    // Update 1.12.0: Extended Download Mods Menu
    public static IDictionaryResource<string> DownloadPackagesSource { get; set; }
}