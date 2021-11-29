using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Commands.Generic.Mod
{
    public class EditModCommand : WithCanExecuteChanged, ICommand
    {
        private readonly PathTuple<ModConfig> _modTuple;
        private DependencyObject _parent;

        public EditModCommand(PathTuple<ModConfig> modTuple, DependencyObject parent)
        {
            _modTuple = modTuple;
            _parent = parent;
        }

        /* Interface Implementation */

        public bool CanExecute(object? parameter) => _modTuple != null;

        public void Execute(object? parameter)
        {
            var createModDialog   = new EditModDialog(_modTuple, IoC.Get<ApplicationConfigService>(), IoC.Get<ModConfigService>());
            if (_parent != null)
                createModDialog.Owner = Window.GetWindow(_parent);
            
            createModDialog.ShowDialog();
        }
    }
}
