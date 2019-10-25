using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using McMaster.NETCore.Plugins;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Loader.IO.Config;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace Reloaded.Mod.Launcher.Commands.ApplicationConfigurationPage
{
    public class ConfigureModCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private static Type[] _sharedTypes = { typeof(IConfigurator) };

        private readonly ApplicationSummaryViewModel _summaryViewModel;
        private PluginLoader _loader;
        private IConfigurator _configurator;

        public ConfigureModCommand(ApplicationSummaryViewModel summaryViewModel)
        {
            _summaryViewModel = summaryViewModel;
            _summaryViewModel.PropertyChanged += SummaryViewModelOnPropertyChanged;
        }

        ~ConfigureModCommand()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            _configurator = null;
            _loader?.Dispose();
            _summaryViewModel.PropertyChanged -= SummaryViewModelOnPropertyChanged;
            GC.Collect();
        }

        /* Implementation */
        private void SummaryViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_summaryViewModel.SelectedMod))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* ICommand */

        public void Execute(object parameter)
        {
            if (!_configurator.TryRunCustomConfiguration())
            {
                var window = new ConfigureModDialog(_configurator.GetConfigurations());
                window.ShowDialog();
            }
        }

        public bool CanExecute(object parameter)
        {
            DisposeCurrent();
            var selectedMod = _summaryViewModel.SelectedMod;
            if (selectedMod != null)
            {
                var config = selectedMod.Generic.ModConfig;

                string dllPath = ModConfig.GetDllPath(selectedMod.Generic.ModConfigPath, (ModConfig) config);
                if (! File.Exists(dllPath))
                    return false;

                _loader = PluginLoader.CreateFromAssemblyFile(dllPath, true, _sharedTypes);
                var assembly    = _loader.LoadDefaultAssembly();
                var types       = assembly.GetTypes();
                var entryPoint  = types.FirstOrDefault(t => typeof(IConfigurator).IsAssignableFrom(t) && !t.IsAbstract);

                if (entryPoint != null)
                {
                    _configurator = (IConfigurator) Activator.CreateInstance(entryPoint);
                    _configurator.SetModDirectory(Path.GetFullPath(Path.GetDirectoryName(selectedMod.Generic.ModConfigPath)));
                    return true;
                }

                _loader?.Dispose();
                return false;
            }

            return false;
        }

        private void DisposeCurrent()
        {
            _configurator = null;
            _loader?.Dispose();
            GC.Collect();
        }
    }
}
