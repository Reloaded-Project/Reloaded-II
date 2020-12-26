using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Commands.EditAppPage
{
    public class DeployAsiLoaderCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private static XamlResource<string> _xamlAsiLoaderDialogTitle = new XamlResource<string>("AsiLoaderDialogTitle");
        private static XamlResource<string> _xamlAsiLoaderDialogDescription = new XamlResource<string>("AsiLoaderDialogDescription");
        private static XamlResource<string> _xamlAsiLoaderDialogLoaderDeployed = new XamlResource<string>("AsiLoaderDialogLoaderDeployed");
        private static XamlResource<string> _xamlAsiLoaderDialogBootstrapperDeployed = new XamlResource<string>("AsiLoaderDialogBootstrapperDeployed");

        private PathTuple<ApplicationConfig> Application { get; set; }
        private AsiLoaderDeployer Deployer { get; set; }
        private EditAppViewModel ViewModel { get; set; }

        public DeployAsiLoaderCommand(EditAppViewModel addAppViewModel)
        {
            ViewModel = addAppViewModel;
            ViewModel.PropertyChanged += OnApplicationChanged;
            SetCurrentApplication();
        }

        public void Dispose()
        {
            UnSetCurrentApplication();
            ViewModel.PropertyChanged -= OnApplicationChanged;
            GC.SuppressFinalize(this);
        }

        /* Implementation */
        private void SetCurrentApplication()
        {
            Application = ViewModel.Application;
            if (Application != null)
            {
                Application.Config.PropertyChanged += LocationPropertyChanged;
                Deployer = new AsiLoaderDeployer(Application);
            }
        }

        private void UnSetCurrentApplication()
        {
            if (Application != null)
                Application.Config.PropertyChanged -= LocationPropertyChanged;
        }

        private void OnApplicationChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Application))
            {
                UnSetCurrentApplication();
                SetCurrentApplication();
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void LocationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Application.Config.AppLocation))
                RaiseCanExecute(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* ICommand */
        public bool CanExecute(object parameter)
        {
            if (Deployer != null && File.Exists(Deployer.Application.Config.AppLocation)) 
                return Deployer.CanDeploy();

            return false;
        }

        public void Execute(object parameter)
        {
            var deployConfirm = new MessageBoxOkCancel(_xamlAsiLoaderDialogTitle.Get(), _xamlAsiLoaderDialogDescription.Get());
            if (deployConfirm.ShowDialog() == true)
            {
                Deployer.DeployAsiLoader(out string loaderPath, out string bootstrapperPath);
                string deployedBootstrapper = $"{_xamlAsiLoaderDialogBootstrapperDeployed.Get()} {bootstrapperPath}";
                if (loaderPath == null)
                {
                    // Installed Bootstrapper but not loader.
                    var box = new MessageBox(_xamlAsiLoaderDialogTitle.Get(), deployedBootstrapper);
                    box.ShowDialog();
                }
                else
                {
                    string deployedLoader = $"{_xamlAsiLoaderDialogLoaderDeployed.Get()} {loaderPath}";
                    var box = new MessageBox(_xamlAsiLoaderDialogTitle.Get(), $"{deployedLoader}\n{deployedBootstrapper}");
                    box.ShowDialog();
                }
            }
        }
    }
}
