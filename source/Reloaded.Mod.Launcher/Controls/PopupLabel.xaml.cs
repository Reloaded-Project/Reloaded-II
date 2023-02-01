namespace Reloaded.Mod.Launcher.Controls;

/// <summary>
/// Interaction logic for PopupLabel.xaml
/// </summary>
public partial class PopupLabel : UserControl
{
    private XamlResource<double> _xamlEntrySlideAnimationDuration = null!;
    private XamlResource<double> _xamlEntryFadeAnimationDuration = null!;
    private XamlResource<double> _xamlEntryFadeOpacityStart = null!;

    private XamlResource<double> _xamlExitSlideAnimationDuration = null!;
    private XamlResource<double> _xamlExitFadeAnimationDuration = null!;
    private XamlResource<double> _xamlExitFadeOpacityEnd = null!;

    public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(nameof(ButtonText), typeof(String), typeof(PopupLabel), new PropertyMetadata("Close Me!"));
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(PopupLabel), new PropertyMetadata(DefaultIsOpen, IsOpenChangedCallback));
    public static readonly DependencyProperty HiddenContentProperty = DependencyProperty.Register(nameof(HiddenContent), typeof(object), typeof(PopupLabel));
    public static readonly DependencyProperty HiddenVisibilityTypeProperty = DependencyProperty.Register(nameof(HiddenVisibilityType), typeof(Visibility), typeof(PopupLabel), new PropertyMetadata(Visibility.Collapsed));

    /* Uses zero width spaces at end to ensure does not clash with real text. */
    private const string DisabledLeft = "▼ ​";
    private const string DisabledRight = " ▼​";
    private const string EnabledLeft  = "▲ ​";
    private const string EnabledRight = " ▲​";
    private const bool DefaultIsOpen = true;

    private bool _isWrapped = false;

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

    /// <summary>
    /// Executed when the <see cref="IsOpen"/> property changes.
    /// </summary>
    public EventHandler<DependencyPropertyChangedEventArgs>? IsOpenChanged { get; set; }

    public PopupLabel()
    {
        InitializeComponent();

        // Merge application resource dictionary (necessary to access XAML settings)
        SetupXamlResources();

        // Update text and whether content is visible on load.
        this.Loaded += (sender, args) =>
        {
            IsOpenChangedCallback(this, new DependencyPropertyChangedEventArgs(IsOpenProperty, DefaultIsOpen, IsOpen));

            if (IsOpen)
                AnimateIn();
            else
                this.HiddenContentContainer.Visibility = HiddenVisibilityType;
        };
    }

    private void SetupXamlResources()
    {
        var thisArray = new[] {this};
        _xamlEntrySlideAnimationDuration = new XamlResource<double>("PopupLabelSlideAnimationDuration", thisArray, this);
        _xamlEntryFadeAnimationDuration = new XamlResource<double>("PopupLabelFadeAnimationDuration", thisArray, this);
        _xamlEntryFadeOpacityStart = new XamlResource<double>("PopupLabelFadeOpacityStart", thisArray, this);

        _xamlExitSlideAnimationDuration = new XamlResource<double>("PopupLabelExitSlideAnimationDuration", thisArray, this);
        _xamlExitFadeAnimationDuration = new XamlResource<double>("PopupLabelExitFadeAnimationDuration", thisArray, this);
        _xamlExitFadeOpacityEnd = new XamlResource<double>("PopupLabelExitFadeOpacityEnd", thisArray, this);
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        // Close/Open
        IsOpen = !IsOpen;

        if (IsOpen)
            AnimateIn();
        else
            AnimateOut();
    }

    private void AnimateOut()
    {
        var animations = new Animation[]
        {
            new RenderTransformAnimation(-this.HiddenContentContainer.ActualHeight, RenderTransformDirection.Vertical, RenderTransformTarget.Away, null, _xamlExitSlideAnimationDuration.Get()),
            new OpacityAnimation(_xamlExitFadeAnimationDuration.Get(), 1, _xamlExitFadeOpacityEnd.Get())
        };

        Animation.Animate(animations, this.HiddenContentContainer);
        this.HiddenContentContainer.Visibility = HiddenVisibilityType;
    }

    private void AnimateIn()
    {
        this.HiddenContentContainer.Visibility = Visibility.Visible;
        var animations = new Animation[]
        {
            new RenderTransformAnimation(-this.HiddenContentContainer.ActualHeight, RenderTransformDirection.Vertical, RenderTransformTarget.Towards, null, _xamlEntrySlideAnimationDuration.Get()),
            new OpacityAnimation(_xamlEntryFadeAnimationDuration.Get(), _xamlEntryFadeOpacityStart.Get(), 1)
        };

        Animation.Animate(animations, this.HiddenContentContainer);
    }

    private static void IsOpenChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PopupLabel label && (bool) e.NewValue != (bool) e.OldValue)
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
            label.IsOpenChanged?.Invoke(label, e);
        }
    }
}