using System;
using System.Diagnostics;
using Reloaded.Mod.Launcher.Commands.EditAppPage;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages
{
    public class EditAppViewModel : ObservableObject, IDisposable
    {
        public PathTuple<ApplicationConfig> Application { get; set; }
        public ApplicationConfigService AppConfigService { get; set; }
        
        public DeleteApplicationCommand DeleteApplicationCommand { get; set; }
        public SetApplicationImageCommand SetApplicationImageCommand { get; set; }
        public DeployAsiLoaderCommand DeployAsiLoaderCommand { get; set; }

        public EditAppViewModel(ApplicationConfigService appConfigService, ApplicationViewModel model)
        {
            Application = model.ApplicationTuple;
            AppConfigService = appConfigService;
            DeleteApplicationCommand = new DeleteApplicationCommand(this);
            DeployAsiLoaderCommand = new DeployAsiLoaderCommand(this);
            SetApplicationImageCommand = new SetApplicationImageCommand(this);
        }

        public void SaveSelectedItem()
        {
            try { Application?.Save(); }
            catch (Exception) { Debug.WriteLine($"{nameof(EditAppViewModel)}: Failed to save current selected item."); }
        }

        public void SetAppImage()
        {
            if (!SetApplicationImageCommand.CanExecute(null)) 
                return;

            SetApplicationImageCommand.Execute(null);
        }

        public void Dispose()
        {
            DeleteApplicationCommand?.Dispose();
            DeployAsiLoaderCommand?.Dispose();
        }
    }
}
    