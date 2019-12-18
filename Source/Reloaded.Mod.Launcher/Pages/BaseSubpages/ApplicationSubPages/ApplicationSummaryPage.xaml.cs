using System;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages
{
    /// <summary>
    /// Interaction logic for ApplicationSummaryPage.xaml
    /// </summary>
    public partial class ApplicationSummaryPage : ApplicationSubPage, IDisposable
    {
        public ApplicationSummaryViewModel ViewModel { get; set; }
        private readonly ResourceManipulator _manipulator;
        private readonly CollectionViewSource _modsViewSource;

        public ApplicationSummaryPage()
        {
            InitializeComponent();
            ViewModel = IoC.Get<ApplicationSummaryViewModel>();

            _manipulator = new ResourceManipulator(Contents);
            _modsViewSource = _manipulator.Get<CollectionViewSource>("FilteredMods");
            _modsViewSource.Filter += ModsViewSourceOnFilter;
            AnimateOutFinished += Dispose;
        }

        ~ApplicationSummaryPage()
        {
            Dispose();
        }

        public void Dispose()
        {
            _modsViewSource.Filter -= ModsViewSourceOnFilter;
            AnimateOutFinished -= Dispose;
            ViewModel?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ModsViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (ModsFilter.Text.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanGenericTuple<ImageModPathTuple>)e.Item;
            e.Accepted = tuple.Generic.ModConfig.ModName.IndexOf(ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _modsViewSource.View.Refresh();
        }
    }
}
