using System;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Commands.Dialog
{
    public class OpenDocumentationCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => ProcessExtensions.OpenFileWithDefaultProgram("https://reloaded-project.github.io/Reloaded-II/");

        #pragma warning disable 67
        public event EventHandler CanExecuteChanged;
        #pragma warning restore 67
    }
}
