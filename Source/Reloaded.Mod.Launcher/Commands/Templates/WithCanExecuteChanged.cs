using System;
using System.Collections.Specialized;
using System.Windows;

namespace Reloaded.Mod.Launcher.Commands.Templates
{
    public abstract class WithCanExecuteChanged
    {
        protected void RaiseCanExecute(object sender, NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => { CanExecuteChanged(sender, e); });
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };
    }
}
