using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

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
            // Problem: EditAppPage.Dispose saves the item.
            IoC.Get<MainPageViewModel>().Page = Pages.BaseSubpages.BaseSubPage.SettingsPage;

            // Find Application in Viewmodel's list.
            var app   = _editAppViewModel.Application.Config;
            var entry = _editAppViewModel.AppConfigService.Items.First(x => x.Config.Equals(app));
            _editAppViewModel.Application = null;

            // Delete folder.
            var directory = Path.GetDirectoryName(entry.Path) ?? throw new InvalidOperationException(Errors.FailedToGetDirectoryOfApplication());
            Directory.Delete(directory, true);

        }
    }
}
