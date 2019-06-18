using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ApplicationSummaryViewModel : ObservableObject
    {
        public BooleanModTuple[] AllMods { get; set; }
        public ImageApplicationPathTuple ApplicationTuple { get; set; }

        public ApplicationSummaryViewModel(ApplicationViewModel model)
        {
            model.PropertyChanged += ModelOnPropertyChanged;
            ApplicationTuple = model.ApplicationTuple;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ApplicationViewModel.ModsForThisAppPropertyName)
            {
                var viewModel = (ApplicationViewModel)sender;
                BuildModsForThisApp(viewModel.ModsForThisApp);
            }
        }

        private void BuildModsForThisApp(IEnumerable<ImageModPathTuple> tuples)
        {
            
        }
    }
}
