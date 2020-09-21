using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Commands.DownloadModsPage
{
    public class ConfigureNuGetSourcesCommand : ICommand
    {
        /// <inheritdoc />
        public bool CanExecute(object? parameter) => true;

        /// <inheritdoc />
        public void Execute(object? parameter)
        {
            var dialog = new ConfigureNuGetFeedsDialog(IoC.Get<LoaderConfig>());
            dialog.ShowDialog();
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;
    }
}
