using System;
using System.IO;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Commands.Generic.Mod
{
    public class DeleteModCommand : WithCanExecuteChanged, ICommand
    {
        private readonly PathTuple<ModConfig> _modTuple;

        public DeleteModCommand(PathTuple<ModConfig> modTuple)
        {
            _modTuple = modTuple;
        }

        /* ICommand. */
        public bool CanExecute(object parameter) => _modTuple != null;

        public void Execute(object parameter)
        {
            // Delete folder contents.
            var directory = Path.GetDirectoryName(_modTuple.Path) ?? throw new InvalidOperationException(Errors.FailedToGetDirectoryOfMod());
            Directory.Delete(directory, true);
        }
    }
}
