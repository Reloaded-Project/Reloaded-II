using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Config.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfigureNuGetFeedsDialog.xaml
    /// </summary>
    public partial class ConfigureNuGetFeedsDialog : ReloadedWindow, IDisposable
    {
        public ConfigureNuGetFeedsDialogViewModel RealViewModel { get; set; }

        public ConfigureNuGetFeedsDialog(LoaderConfig loaderConfig)
        {
            InitializeComponent();
            RealViewModel = new ConfigureNuGetFeedsDialogViewModel(loaderConfig);
            this.Closed += SaveOnClose;
        }

        /// <inheritdoc />
        public void Dispose() => RealViewModel?.Dispose();

        private void SaveOnClose(object sender, EventArgs e)
        {
            RealViewModel.Save();
            Dispose();
        }
    }

    public class ConfigureNuGetFeedsDialogViewModel : ObservableObject, IDisposable
    {
        public CreateNewFeedCommand CreateNewFeedCommand { get; set; }
        public DeleteFeedCommand    DeleteFeedCommand    { get; set; }

        public NugetFeed CurrentFeed                  { get; set; }
        public ObservableCollection<NugetFeed> Feeds  { get; set; }

        public bool IsEnabled => CurrentFeed != null;

        private LoaderConfig _config                  { get; set; }

        /// <inheritdoc />
        public ConfigureNuGetFeedsDialogViewModel(LoaderConfig loaderConfig)
        {
            _config                 = loaderConfig;
            Feeds                   = new ObservableCollection<NugetFeed>(loaderConfig.NuGetFeeds);
            CreateNewFeedCommand    = new CreateNewFeedCommand(this);
            DeleteFeedCommand       = new DeleteFeedCommand(this);
        }

        /// <inheritdoc />
        public void Dispose() => DeleteFeedCommand?.Dispose();

        /// <summary>
        /// Writes the configuration to loader config and saves the config.
        /// </summary>
        public void Save()
        {
            _config.NuGetFeeds = Feeds.Where(x => !string.IsNullOrEmpty(x.URL)).ToArray();
            IConfig<LoaderConfig>.ToPathAsync(_config, Paths.LoaderConfigPath);
            IoC.Get<AggregateNugetRepository>().FromFeeds(Feeds);
        }
    }

    public class CreateNewFeedCommand : ICommand
    {
        public ConfigureNuGetFeedsDialogViewModel ViewModel { get; set; }
        public CreateNewFeedCommand(ConfigureNuGetFeedsDialogViewModel viewModel) => ViewModel = viewModel;

        /// <inheritdoc />
        public bool CanExecute(object parameter) => true;

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            var feed = new NugetFeed("New NuGet Feed", "");
            ViewModel.Feeds.Add(feed);
            ViewModel.CurrentFeed = feed;
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;
    }

    public class DeleteFeedCommand : ICommand, IDisposable
    {
        public ConfigureNuGetFeedsDialogViewModel ViewModel { get; set; }
        public DeleteFeedCommand(ConfigureNuGetFeedsDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        /// <inheritdoc />
        public void Dispose() => ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentFeed))
                CanExecuteChanged?.Invoke(sender, e);
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter) => ViewModel.CurrentFeed != null;

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            if (ViewModel.CurrentFeed == null) 
                return;

            ViewModel.Feeds.Remove(ViewModel.CurrentFeed);
            ViewModel.CurrentFeed = ViewModel.Feeds.FirstOrDefault();
        }

        /// <inheritdoc />
        public event EventHandler CanExecuteChanged;
    }
}
