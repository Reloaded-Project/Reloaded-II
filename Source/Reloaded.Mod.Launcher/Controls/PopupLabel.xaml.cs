using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Reloaded.WPF.Pages.Animations;
using Reloaded.WPF.Pages.Animations.Enum;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Controls
{
    /// <summary>
    /// Interaction logic for PopupLabel.xaml
    /// </summary>
    public partial class PopupLabel : UserControl
    {
        #region XAML Name Constants
        // ReSharper disable InconsistentNaming
        private const string XAML_PopupLabelSlideAnimationDuration = "PopupLabelSlideAnimationDuration";
        private const string XAML_PopupLabelFadeAnimationDuration = "PopupLabelFadeAnimationDuration";
        private const string XAML_PopupLabelFadeOpacityStart = "PopupLabelFadeOpacityStart";

        private const string XAML_PopupLabelExitSlideAnimationDuration = "PopupLabelExitSlideAnimationDuration";
        private const string XAML_PopupLabelExitFadeAnimationDuration = "PopupLabelExitFadeAnimationDuration";
        private const string XAML_PopupLabelExitFadeOpacityEnd = "PopupLabelExitFadeOpacityEnd";
        // ReSharper restore InconsistentNaming
        #endregion

        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(nameof(ButtonText), typeof(String), typeof(PopupLabel), new PropertyMetadata("Close Me!"));
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(PopupLabel), new PropertyMetadata(true, IsOpenChangedCallback));
        public static readonly DependencyProperty HiddenContentProperty = DependencyProperty.Register(nameof(HiddenContent), typeof(object), typeof(PopupLabel));
        public static readonly DependencyProperty HiddenVisibilityTypeProperty = DependencyProperty.Register(nameof(HiddenVisibilityType), typeof(Visibility), typeof(PopupLabel), new PropertyMetadata(System.Windows.Visibility.Collapsed));

        /* Uses zero width spaces at end to ensure does not clash with real text. */
        private const string DisabledLeft = "▼ ​";
        private const string DisabledRight = " ▼​";
        private const string EnabledLeft  = "▲ ​";
        private const string EnabledRight = " ▲​";

        private bool _isWrapped = false;
        private ResourceManipulator _resourceManipulator;

        /// <summary>
        /// Gets or Sets the text displayed by the label.
        /// </summary>
        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        /// <summary>
        /// Declares whether is open.
        /// </summary>
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        /// <summary>
        /// Sets the hidden/visible content of the popup label.
        /// </summary>
        public object HiddenContent
        {
            get => (bool)GetValue(HiddenContentProperty);
            set => SetValue(HiddenContentProperty, value);
        }

        /// <summary>
        /// Declares whether the expandable content should be declared as <see cref="Visibility.Hidden"/> or <see cref="Visibility.Collapsed"/>
        /// when not shown.
        /// </summary>
        public Visibility HiddenVisibilityType
        {
            get => (Visibility)GetValue(HiddenVisibilityTypeProperty);
            set => SetValue(HiddenVisibilityTypeProperty, value);
        }

        public PopupLabel()
        {
            InitializeComponent();

            // Merge application resource dictionary (necessary to access XAML settings)
            this.Resources.MergedDictionaries.Add(Application.Current.Resources);
            _resourceManipulator = new ResourceManipulator(this);

            // Update text and whether content is visible on load.
            this.Loaded += (sender, args) =>
            {
                IsOpenChangedCallback(this, new DependencyPropertyChangedEventArgs(IsOpenProperty, !IsOpen, IsOpen));

                if (IsOpen)
                    AnimateIn();
                else
                    this.HiddenContentContainer.Visibility = HiddenVisibilityType;
            };
        }

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Close/Open
            IsOpen = !IsOpen;

            if (IsOpen)
                AnimateIn();
            else
                AnimateOut();
        }

        private async void AnimateOut()
        {
            var animations = new Animation[]
            {
                new RenderTransformAnimation(-this.HiddenContentContainer.ActualHeight, RenderTransformDirection.Vertical, RenderTransformTarget.Away, null, _resourceManipulator.Get<double>(XAML_PopupLabelExitSlideAnimationDuration)),
                new OpacityAnimation(_resourceManipulator.Get<double>(XAML_PopupLabelExitFadeAnimationDuration), 1, _resourceManipulator.Get<double>(XAML_PopupLabelExitFadeOpacityEnd))
            };

            await Animation.AnimateAsync(animations, this.HiddenContentContainer);
            this.HiddenContentContainer.Visibility = HiddenVisibilityType;
        }

        private async void AnimateIn()
        {
            this.HiddenContentContainer.Visibility = Visibility.Visible;
            var animations = new Animation[]
            {
                new RenderTransformAnimation(-this.HiddenContentContainer.ActualHeight, RenderTransformDirection.Vertical, RenderTransformTarget.Towards, null, _resourceManipulator.Get<double>(XAML_PopupLabelSlideAnimationDuration)),
                new OpacityAnimation(_resourceManipulator.Get<double>(XAML_PopupLabelFadeAnimationDuration), _resourceManipulator.Get<double>(XAML_PopupLabelFadeOpacityStart), 1)
            };

            await Animation.AnimateAsync(animations, this.HiddenContentContainer);
        }

        private static void IsOpenChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PopupLabel label)
            {
                string originalText = label.ButtonText;
                if (label._isWrapped)
                {
                    originalText = (bool) e.OldValue == false ? 
                        originalText.Substring(DisabledLeft.Length, originalText.Length - DisabledLeft.Length - DisabledRight.Length) : 
                        originalText.Substring(EnabledLeft.Length, originalText.Length - EnabledLeft.Length - EnabledRight.Length);
                }

                label.ButtonText = label.IsOpen ? 
                    $"{EnabledLeft}{originalText}{EnabledRight}" : 
                    $"{DisabledLeft}{originalText}{DisabledRight}";

                label._isWrapped = true;
            }
        }
    }
}
