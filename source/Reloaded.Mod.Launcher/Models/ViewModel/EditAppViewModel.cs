using System;
using Reloaded.Mod.Launcher.Commands.EditAppPage;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class EditAppViewModel : ObservableObject, IDisposable
    {
        public ImageApplicationPathTuple Application { get; set; }
        public MainPageViewModel MainPageViewModel { get; set; }
        public DeleteApplicationCommand DeleteApplicationCommand { get; set; }
        public SetApplicationImageCommand SetApplicationImageCommand { get; set; }
        public DeployAsiLoaderCommand DeployAsiLoaderCommand { get; set; }

        public EditAppViewModel(MainPageViewModel viewModel, ApplicationViewModel model)
        {
            Application = model.ApplicationTuple;
            MainPageViewModel = viewModel;
            DeleteApplicationCommand = new DeleteApplicationCommand(this);
            DeployAsiLoaderCommand = new DeployAsiLoaderCommand(this);
            SetApplicationImageCommand = new SetApplicationImageCommand(this);
        }

        public void RaiseApplicationChangedEvent() => RaisePropertyChangedEvent(nameof(Application));

        public void Dispose()
        {
            DeleteApplicationCommand?.Dispose();
            DeployAsiLoaderCommand?.Dispose();
        }
    }
}
    