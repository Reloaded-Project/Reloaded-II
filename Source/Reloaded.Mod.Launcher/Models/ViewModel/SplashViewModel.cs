using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class SplashViewModel : ObservableObject
    {
        public string Text { get; set; }
    }
}
