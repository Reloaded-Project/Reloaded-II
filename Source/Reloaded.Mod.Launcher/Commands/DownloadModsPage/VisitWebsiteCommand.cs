using System;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Commands.DownloadModsPage
{
    public class VisitWebsiteCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            ProcessExtensions.OpenFileWithDefaultProgram(SharedConstants.NuGetApiWebsite);
        }

        public event EventHandler CanExecuteChanged;
    }
}