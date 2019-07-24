using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Commands.Dialog
{
    public class OpenDocumentationCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => ProcessExtensions.OpenFileWithDefaultProgram("https://github.com/Reloaded-Project/Reloaded-II");
        public event EventHandler CanExecuteChanged;
    }
}
