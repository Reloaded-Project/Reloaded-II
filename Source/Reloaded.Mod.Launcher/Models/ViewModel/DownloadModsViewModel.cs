using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PropertyChanged;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.Model.DownloadModsPage;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class DownloadModsViewModel : ObservableObject
    {
        public string SearchQuery                                         { get; set; }
        public ObservableCollection<DownloadModEntry> DownloadModEntries  { get; set; }
        public DownloadModEntry                       DownloadModEntry    { get; set; }
        public DownloadModStatus                      DownloadModStatus   { get; set; }

        private NugetHelper             _nugetHelper;
        private ManageModsViewModel     _manageModsViewModel;
        private CancellationTokenSource _tokenSource;

        /* Construction - Deconstruction */
        public DownloadModsViewModel(ManageModsViewModel manageModsViewModel, NugetHelper nugetHelper)
        {
            _nugetHelper = nugetHelper;
            _manageModsViewModel = manageModsViewModel;
            DownloadModEntries = new ObservableCollection<DownloadModEntry>();
            PropertyChanged += OnSearchQueryChanged;
            #pragma warning disable 4014
            GetSearchResults(); // Fire and forget.
            #pragma warning restore 4014
        }

        /* Business Logic */
        private async Task GetSearchResults()
        {
            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();

            var searchResults = await _nugetHelper.Search(SearchQuery, false, 50, _tokenSource.Token);
            var modEntries = searchResults.Select(x => new DownloadModEntry(x.Identity.Id, x.Authors, x.Description, x.Identity.Version));
            Collections.ModifyObservableCollection(DownloadModEntries, modEntries);
        }

        private void OnSearchQueryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchQuery))
                #pragma warning disable 4014
                GetSearchResults(); // Fire and forget.
                #pragma warning restore 4014
        }
    }
}
