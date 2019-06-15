using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Reloaded.Mod.Launcher.Commands.Dialog
{
    public class OpenUserGuideCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => Process.Start("https://github.com/Reloaded-Project/Reloaded-II");
        public event EventHandler CanExecuteChanged;
    }
}
