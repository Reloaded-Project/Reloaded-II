using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.WPF.Pages.Animations;
using Reloaded.WPF.Pages.Animations.Enum;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : ReloadedIIPage, IDisposable
    {
        public ApplicationViewModel ViewModel { get; set; }

        public ApplicationPage()
        {
            InitializeComponent();
            ViewModel = IoC.GetConstant<ApplicationViewModel>();
            this.AnimateOutStarted += Dispose;
        }

        ~ApplicationPage()
        {
            Dispose();
        }

        public void Dispose()
        {
            ViewModel?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates instances of the animations that are ran on exiting the page.
        /// Note: Override this to modify animations used by page.
        /// </summary>
        protected override Animation[] MakeExitAnimations()
        {
            return new Animation[]
            {
                new RenderTransformAnimation(-this.ActualWidth, RenderTransformDirection.Horizontal, RenderTransformTarget.Away, null, ResourceManipulator.Get<double>(XAML_EXITSLIDEANIMATIONDURATION)),
                new OpacityAnimation(ResourceManipulator.Get<double>(XAML_EXITFADEANIMATIONDURATION), 1, ResourceManipulator.Get<double>(XAML_EXITFADEOPACITYEND))
            };
        }

        protected override void OnAnimateInFinished()
        {
            if (!String.IsNullOrEmpty(this.Title))
                IoC.Get<WindowViewModel>().WindowTitle = $"{this.Title}: {ViewModel.ApplicationTuple.ApplicationConfig.AppName}";
        }
    }
}
