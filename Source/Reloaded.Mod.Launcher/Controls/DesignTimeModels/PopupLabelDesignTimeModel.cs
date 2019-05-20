using System;
using System.Windows.Controls;

namespace Reloaded.Mod.Launcher.Controls.DesignTimeModels
{
    public class PopupLabelDesignTimeModel
    {
        public static PopupLabelDesignTimeModel Instance { get; set; } = new PopupLabelDesignTimeModel();
        public String ButtonText { get; set; } = "Close Me!";
        public bool IsOpen { get; set; } = true;
        public object HiddenContent { get; set; } = new Button() { Content = "Sample Content" };
    }
}
