using System;
using System.Collections.Specialized;
using System.Windows;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Commands.Templates
{
    public abstract class WithCanExecuteChanged
    {
        protected void RaiseCanExecute(object sender, NotifyCollectionChangedEventArgs e)
        {
            ActionWrappers.ExecuteWithApplicationDispatcher(() => CanExecuteChanged(sender, e));
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };
    }
}
