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
                new RenderTransformAnimation(this.ActualWidth, RenderTransformDirection.Horizontal, RenderTransformTarget.Towards, null, XamlEntrySlideAnimationDuration.Get()),
                new OpacityAnimation(XamlEntryFadeAnimationDuration.Get(), XamlEntryFadeOpacityStart.Get(), 1)
            };
        }

        protected override Animation[] MakeExitAnimations()
        {
            return new Animation[]
            {
                new RenderTransformAnimation(this.ActualWidth, RenderTransformDirection.Horizontal, RenderTransformTarget.Away, null, XamlExitSlideAnimationDuration.Get()),
                new OpacityAnimation(XamlExitFadeAnimationDuration.Get(), 1, XamlExitFadeOpacityEnd.Get())
            };
        }
    }
}
