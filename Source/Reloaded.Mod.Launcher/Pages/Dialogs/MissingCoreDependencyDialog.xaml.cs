using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Reloaded.Mod.Loader.IO.Weaving;
using Reloaded.Mod.Loader.Update.Dependency;
using Reloaded.Mod.Loader.Update.Dependency.Interfaces;
using Reloaded.Mod.Shared;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for MissingCoreDependencyDialog.xaml
    /// </summary>
    public partial class MissingCoreDependencyDialog : ReloadedWindow
    {
        public MissingCoreDependencyDialogViewModel ViewModel { get; set; }

        public MissingCoreDependencyDialog(DependencyChecker deps)
        {
            ViewModel = new MissingCoreDependencyDialogViewModel(deps);
            InitializeComponent();
        }

        private void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            var model = (DependencyModel)((FrameworkElement)sender).DataContext;
            model.OpenUrls();
        }
    }

    public class MissingCoreDependencyDialogViewModel : ObservableObject
    {
        public ObservableCollection<DependencyModel> Dependencies { get; set; }

        public MissingCoreDependencyDialogViewModel(DependencyChecker deps)
        {
            Dependencies = new ObservableCollection<DependencyModel>(deps.Dependencies.Where(x => !x.Available).Select(x => new DependencyModel(x)));
        }
    }

    public class DependencyModel
    {
        public string Name    => Dependency.Name;
        public bool IsMissing => !Dependency.Available;
        public IDependency Dependency;

        public DependencyModel(IDependency dependency)
        {
            Dependency = dependency;
        }

        public void OpenUrls()
        {
            foreach (var url in Dependency.GetUrls()) 
                ProcessExtensions.OpenFileWithDefaultProgram(url);
        }
    }
}
