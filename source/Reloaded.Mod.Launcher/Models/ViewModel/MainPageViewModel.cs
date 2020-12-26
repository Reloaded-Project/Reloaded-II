using PropertyChanged;
using Reloaded.Mod.Launcher.Commands.EditAppPage;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using ObservableObject = Reloaded.WPF.MVVM.ObservableObject;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class MainPageViewModel : ObservableObject
    {
        /* The page to display. */
        [DoNotNotify]
        public BaseSubPage Page
        {
            get => _baseSubPage;
            set
            {
                if (_baseSubPage == value) 
                    return;

                _baseSubPage = value;
                RaisePropertyChangedEvent(nameof(Page));
            }
        }

        /* Set this to false to temporarily suspend the file system watcher monitoring new applications. */
        public PathTuple<ApplicationConfig> SelectedApplication { get; private set; }

        /* Allows us to add an application */
        public AddApplicationCommand AddApplicationCommand      { get; private set; }

        /* Service Providing Access to Applications */
        public ApplicationConfigService ConfigService           { get; private set; }

        /* This one is to allow us to switch application without raising event twice. See: SwitchApplication */
        private BaseSubPage _baseSubPage = BaseSubPage.SettingsPage;

        /* Application Monitoring */
        private AutoInjector _autoInjector;

        public MainPageViewModel(ApplicationConfigService service)
        {
            ConfigService = service;
            AddApplicationCommand = new AddApplicationCommand(this, service);
            _autoInjector = new AutoInjector(service);
        }

        /// <summary>
        /// Changes the Application page to display and displays the application.
        /// </summary>
        public void SwitchToApplication(PathTuple<ApplicationConfig> tuple)
        {
            SelectedApplication = tuple;
            _baseSubPage = BaseSubPage.Application;
            RaisePropertyChangedEvent(nameof(Page));
        }
    }
}
