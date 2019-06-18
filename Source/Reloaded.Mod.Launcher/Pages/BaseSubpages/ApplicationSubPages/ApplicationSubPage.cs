using Reloaded.WPF.Pages.Animations;
using Reloaded.WPF.Pages.Animations.Enum;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages
{
    /// <summary>
    /// Base class with animation set for sub-pages of <see cref="ApplicationPage"/>
    /// </summary>
    public class ApplicationSubPage : ReloadedIIPage
    {
        protected override Animation[] MakeEntryAnimations()
        {
            return new Animation[]
            {
                new RenderTransformAnimation(this.ActualWidth, RenderTransformDirection.Horizontal, RenderTransformTarget.Towards, null, ResourceManipulator.Get<double>(XAML_ENTRYSLIDEANIMATIONDURATION)),
                new OpacityAnimation(ResourceManipulator.Get<double>(XAML_ENTRYFADEANIMATIONDURATION), ResourceManipulator.Get<double>(XAML_ENTRYFADEOPACITYSTART), 1)
            };
        }

        protected override Animation[] MakeExitAnimations()
        {
            return new Animation[]
            {
                new RenderTransformAnimation(this.ActualWidth, RenderTransformDirection.Horizontal, RenderTransformTarget.Away, null, ResourceManipulator.Get<double>(XAML_EXITSLIDEANIMATIONDURATION)),
                new OpacityAnimation(ResourceManipulator.Get<double>(XAML_EXITFADEANIMATIONDURATION), 1, ResourceManipulator.Get<double>(XAML_EXITFADEOPACITYEND))
            };
        }
    }
}
