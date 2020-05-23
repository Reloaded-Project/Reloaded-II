using System;
using Reloaded.Mod.Launcher.Commands.AddAppPage;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class AddAppViewModel : ObservableObject, IDisposable
    {
        public ImageApplicationPathTuple Application { get; set; }
        public MainPageViewModel MainPageViewModel { get; set; }
        public int SelectedIndex { get; set; } = 0;
        public AddApplicationCommand AddApplicationCommand { get; set; }
        public DeleteApplicationCommand DeleteApplicationCommand { get; set; }
        public SetApplicationImageCommand SetApplicationImageCommand { get; set; }
        public DeployAsiLoaderCommand DeployAsiLoaderCommand { get; set; }

        public AddAppViewModel(MainPageViewModel viewModel)
        {
            MainPageViewModel = viewModel;
            AddApplicationCommand = new AddApplicationCommand(this);
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
