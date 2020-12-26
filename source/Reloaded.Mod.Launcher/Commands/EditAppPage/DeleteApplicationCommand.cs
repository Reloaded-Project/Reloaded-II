using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Commands.EditAppPage
{
    /// <summary>
    /// Command to be used by the <see cref="EditAppPage"/> which decides
    /// whether the current entry can be removed.
    /// </summary>
    public class DeleteApplicationCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private readonly EditAppViewModel _editAppViewModel;

        public DeleteApplicationCommand(EditAppViewModel editAppViewModel)
        {
            _editAppViewModel = editAppViewModel;
        }

        ~DeleteApplicationCommand()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        
        /* ICommand. */

        public bool CanExecute(object parameter)
        {
            return _editAppViewModel.Application != null;
        }

        public void Execute(object parameter)
        {
            // Find Application in Viewmodel's list and remove it.
            var app   = _editAppViewModel.Application.Config;
            var entry = _editAppViewModel.AppConfigService.Applications.First(x => x.Config.Equals(app));
            _editAppViewModel.AppConfigService.Applications.Remove(entry);

            // Delete folder contents.
            var directory = Path.GetDirectoryName(entry.Path) ?? throw new InvalidOperationException(Errors.FailedToGetDirectoryOfApplication());
            Directory.Delete(directory, true);

            // File system watcher automatically updates collection in MainPageViewModel.Applications
            IoC.Get<MainPageViewModel>().Page = Pages.BaseSubpages.BaseSubPage.SettingsPage;
        }
    }
}
