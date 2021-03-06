﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Commands.ApplicationConfigurationPage
{
    public class OpenModFolderCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private readonly AppSummaryViewModel _summaryViewModel;

        public OpenModFolderCommand(AppSummaryViewModel summaryViewModel)
        {
            _summaryViewModel = summaryViewModel;
            _summaryViewModel.PropertyChanged += SummaryViewModelPropertyChanged;
        }

        ~OpenModFolderCommand()
        {
            Dispose();
        }

        public void Dispose()
        {
            _summaryViewModel.PropertyChanged -= SummaryViewModelPropertyChanged;
            GC.SuppressFinalize(this);
        }

        /* Implementation */

        private void SummaryViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_summaryViewModel.SelectedMod))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* ICommand */

        public bool CanExecute(object parameter)
        {
            if (_summaryViewModel.SelectedMod != null)
            {
                string directoryPath = Path.GetDirectoryName(_summaryViewModel.SelectedMod.Tuple.Path);
                if (Directory.Exists(directoryPath))
                    return true;
            }

            return false;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Execute(object parameter)
        {
            string directoryPath = Path.GetDirectoryName(_summaryViewModel.SelectedMod.Tuple.Path);
            ProcessExtensions.OpenFileWithExplorer(directoryPath);
        }
    }
}
