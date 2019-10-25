using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfigureModDialog.xaml
    /// </summary>
    public partial class ConfigureModDialog : ReloadedWindow
    {
        public new ConfigureModDialogViewModel ViewModel { get; set; }
        public ConfigureModDialog(IConfigurable[] configurables)
        {
            InitializeComponent();
            ViewModel = new ConfigureModDialogViewModel(configurables);
        }

        private void Save_Click(object sender, RoutedEventArgs e) => ViewModel.Save();
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => ViewModel.Save();

        private void PropertyGridMakeExpandable(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemEventArgs e)
        {
            if (e.Item is PropertyItem item)
                if ((item.PropertyType.IsValueType && !item.PropertyType.IsEnum && !item.PropertyType.IsPrimitive) || item.PropertyType.IsClass)
                    item.IsExpandable = true;
        }
    }

    public partial class ConfigureModDialogViewModel : ObservableObject
    {
        public IConfigurable[] Configurables { get; set; }
        public IConfigurable CurrentConfigurable { get; set; }
        public ConfigureModDialogViewModel(IConfigurable[] configurables)
        {
            Configurables = configurables;
            if (Configurables.Length > 0)
                CurrentConfigurable = Configurables[0];
        }

        public void Save()
        {
            foreach (var configurable in Configurables)
                configurable.Save();
        }
    }
}
