using System;
using System.Windows.Input;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Commands.Dialog
{
    public class OpenUserGuideCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => ProcessExtensions.OpenFileWithDefaultProgram("https://github.com/Reloaded-Project/Reloaded-II#reloaded-for-end-users");

        #pragma warning disable 67
        public event EventHandler CanExecuteChanged;
        #pragma warning restore 67
    }
}
