using System;
using System.Windows.Data;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Application;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;

/// <summary>
/// Interaction logic for ApplicationSummaryPage.xaml
/// </summary>
public partial class AppSummaryPage : ApplicationSubPage, IDisposable
{
    public ConfigureModsViewModel ViewModel { get; set; }
    private readonly DictionaryResourceManipulator _manipulator;
    private readonly CollectionViewSource _modsViewSource;

    public AppSummaryPage()
    {
        InitializeComponent();
        ViewModel = IoC.Get<ConfigureModsViewModel>();

        _manipulator    = new DictionaryResourceManipulator(this.Contents.Resources);
        _modsViewSource = _manipulator.Get<CollectionViewSource>("FilteredMods");
        _modsViewSource.Filter += ModsViewSourceOnFilter;
        AnimateOutFinished += Dispose;
    }

    ~AppSummaryPage()
    {
        Dispose();
    }

    public void Dispose()
    {
        ActionWrappers.ExecuteWithApplicationDispatcher(() => _modsViewSource.Filter -= ModsViewSourceOnFilter);
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

        var tuple = (ModEntry)e.Item;
        e.Accepted = tuple.Tuple.Config.ModName.Contains(ModsFilter.Text, StringComparison.InvariantCultureIgnoreCase);
    }

    private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _modsViewSource.View.Refresh();
    }
}