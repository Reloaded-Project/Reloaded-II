using System.Collections.Generic;
using System.ComponentModel;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class ApplicationSummaryViewModel : ObservableObject
    {
        public List<BooleanGenericTuple<IModConfig>> AllMods { get; set; }
        public ImageApplicationPathTuple ApplicationTuple { get; set; }

        public ApplicationSummaryViewModel(ApplicationViewModel model)
        {
            ApplicationTuple = model.ApplicationTuple;
            InitialBuildModList(model.ModsForThisApp);
        }

        private void InitialBuildModList(IEnumerable<ImageModPathTuple> tuples)
        {

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
            foreach (var tuple in tuples)
            {



            }
        }
    }
}
