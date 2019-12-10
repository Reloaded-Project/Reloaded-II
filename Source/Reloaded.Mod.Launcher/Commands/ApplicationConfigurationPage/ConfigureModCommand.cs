using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using McMaster.NETCore.Plugins;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace Reloaded.Mod.Launcher.Commands.ApplicationConfigurationPage
{
    public class ConfigureModCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private static Type[] _sharedTypes = { typeof(IConfigurator) };
        private readonly ApplicationSummaryViewModel _summaryViewModel;

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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _summaryViewModel.PropertyChanged -= SummaryViewModelOnPropertyChanged;
        }

        /* Implementation */
        private void SummaryViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_summaryViewModel.SelectedMod))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* ICommand */

        // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(object parameter)
        {
            // Important Note: We are keeping everything to the stack.
            // Want our best to ensure that no types leak out anywhere making unloadability hard.
            // Also, we must also keep loader used to load the configurator in stack, for obvious reasons.
            if (TryGetConfigurator(_summaryViewModel.SelectedMod, out var configurator, out var loader))
            {
                if (!configurator.TryRunCustomConfiguration())
                {
                    var window = new ConfigureModDialog(configurator.GetConfigurations());
                    window.ShowDialog();
                }
            }
        }

        // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool CanExecute(object parameter)
        {
            var selectedMod = _summaryViewModel.SelectedMod;
            if (selectedMod != null)
                return TryGetConfigurator(selectedMod, out _, out _);

            return false;
        }

        // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryGetConfigurator(BooleanGenericTuple<ImageModPathTuple> selectedMod, out IConfigurator configurator, out PluginLoader loader)
        {
            var config = selectedMod.Generic.ModConfig;
            string dllPath = ModConfig.GetDllPath(selectedMod.Generic.ModConfigPath, (ModConfig)config);
            configurator = null;
            loader = null;

            if (!File.Exists(dllPath))
                return false;

            loader = PluginLoader.CreateFromAssemblyFile(dllPath, true, _sharedTypes);
            var assembly = loader.LoadDefaultAssembly();
            var types = assembly.GetTypes();
            var entryPoint = types.FirstOrDefault(t => typeof(IConfigurator).IsAssignableFrom(t) && !t.IsAbstract);

            if (entryPoint != null)
            {
                configurator = (IConfigurator)Activator.CreateInstance(entryPoint);
                configurator.SetModDirectory(Path.GetFullPath(Path.GetDirectoryName(selectedMod.Generic.ModConfigPath)));
                return true;
            }

            return false;
        }
    }
}
