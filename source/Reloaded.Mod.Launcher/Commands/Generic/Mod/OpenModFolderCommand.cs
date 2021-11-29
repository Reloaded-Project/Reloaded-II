using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;

namespace Reloaded.Mod.Launcher.Commands.Generic.Mod
{
    public class OpenModFolderCommand : ICommand
    {
        private readonly PathTuple<ModConfig> _modTuple;

        public OpenModFolderCommand(PathTuple<ModConfig> modTuple)
        {
            _modTuple = modTuple;
        }
        
        /* ICommand */

        public bool CanExecute(object parameter)
        {
            if (_modTuple != null)
            {
                string directoryPath = Path.GetDirectoryName(_modTuple.Path);
                if (Directory.Exists(directoryPath))
                    return true;
            }

            return false;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Execute(object parameter)
        {
            string directoryPath = Path.GetDirectoryName(_modTuple.Path);
            ProcessExtensions.OpenFileWithExplorer(directoryPath);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
