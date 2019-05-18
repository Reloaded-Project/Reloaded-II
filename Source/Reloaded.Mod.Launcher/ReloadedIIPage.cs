using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Special type of <see cref="ReloadedWindow"/> that adds localization support
    /// to the existing windows as well as auto-changes Window titles.
    /// </summary>
    public class ReloadedIIPage : ReloadedPage
    {
        public ReloadedIIPage()
        {
            this.Loaded += OnLoaded;
        }

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Set language dictionary.
            var langDict = new ResourceDictionary();
            string culture = Thread.CurrentThread.CurrentCulture.ToString();
            string languageFilePath = AppDomain.CurrentDomain.BaseDirectory + $"/Languages/{culture}.xaml";
            if (File.Exists(languageFilePath))
                langDict.Source = new Uri(languageFilePath, UriKind.Absolute);

            this.Resources.MergedDictionaries.Add(langDict);

            // Change window title to current page title.
            if (! String.IsNullOrEmpty(this.Title))
                IoC.Get<MainWindow>().Title = this.Title;
        }
    }
}
