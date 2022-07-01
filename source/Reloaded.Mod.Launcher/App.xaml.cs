using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using HandyControl.Controls;
using Reloaded.Mod.Launcher.Utility;
using static System.Environment;

namespace Reloaded.Mod.Launcher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Entry point for the application.
    /// </summary>
    public App()
    {
        // Update handler
        if (Sewer56.Update.Hooks.Startup.HandleCommandLineArgs(GetCommandLineArgs()))
        {
            Application.Current.Shutdown(0);
            return;
        }

        this.Startup += OnStartup;
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        SetupResources();

        // Common Commandline Argument Handler
        var originalMode = Application.Current.ShutdownMode;
        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (Lib.Startup.HandleCommandLineArgs())
        {
            Application.Current.Shutdown(0);
            return;
        }

        Application.Current.ShutdownMode = originalMode;
    }

    private void SetupResources()
    {
        var launcherFolder   = Path.GetDirectoryName(GetCommandLineArgs()[0]);
        var languageSelector = new XamlFileSelector($"{launcherFolder}\\Assets\\Languages");
        var themeSelector    = new XamlFileSelector($"{launcherFolder}\\Theme");
        
        LibraryBindings.Init(languageSelector, themeSelector);

        // Ideally this should be in Setup, however the download dialogs should be localized.
        Resources.MergedDictionaries.Add(languageSelector);
        Resources.MergedDictionaries.Add(themeSelector);
        themeSelector.NewFileSet += OnThemeChanged;
        Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"{launcherFolder}\\Theme\\Helpers\\BackwardsCompatibilityHelpers.xaml", UriKind.RelativeOrAbsolute) });
    }

    private void OnThemeChanged()
    {
        void TryAssignResource(string originalResource, string targetResource)
        {
            try { Resources[targetResource] = Resources[originalResource]; }
            catch (Exception) { }
        }

        // HandyControl Compatibility
        TryAssignResource("AccentColorLighter", "DarkAccentColor");
        TryAssignResource("AccentColorLight", "SecondaryTitleColor");
        TryAssignResource("AccentColorLight", "TitleColor");
        TryAssignResource("AccentColorLighter", "DarkPrimaryColor");
        TryAssignResource("AccentColorLighter", "PrimaryColor");
        TryAssignResource("AccentColorLight", "LightPrimaryColor");

        // Remove focus from scroll viewers
        if (Current.MainWindow != null)
        {
            Current.MainWindow.ApplyTemplate();
            Current.MainWindow.OnApplyTemplate();
            Current.MainWindow.InvalidateVisual();
        }
    }
}