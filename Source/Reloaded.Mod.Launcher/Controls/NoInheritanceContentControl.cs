using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Reloaded.Mod.Launcher.Controls
{
    public class NoInheritanceContentControl : ContentControl
    {
        //  https://stackoverflow.com/a/2138114
        //  but here skip to theme instead of skipping all
        //  otherwise the SearchPanel is rendered completely translucent
        //
        public NoInheritanceContentControl()
        {
            this.InheritanceBehavior = InheritanceBehavior.SkipToThemeNow;
        }
    }
}
