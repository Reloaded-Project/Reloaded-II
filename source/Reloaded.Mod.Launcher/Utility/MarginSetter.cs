namespace Reloaded.Mod.Launcher.Utility;

// Adapted from, https://gist.github.com/angularsen/90040fb174f71c5ab3ad, originally MIT licensed.
// Improved to support non-visible items.
// Not reusable, hardcoded for this launcher.

public class MarginSetter
{
    private static Thickness GetLastItemMargin(Panel obj) => (Thickness)obj.GetValue(LastItemMarginProperty);

    public static Thickness GetMargin(DependencyObject obj) => (Thickness)obj.GetValue(MarginProperty);

    public static void SetLastItemMargin(DependencyObject obj, Thickness value) => obj.SetValue(LastItemMarginProperty, value);

    public static void SetMargin(DependencyObject obj, Thickness value) => obj.SetValue(MarginProperty, value);

    public static bool GetEnable(DependencyObject obj) => (bool)obj.GetValue(EnableProperty);

    public static void SetEnable(DependencyObject obj, bool value) => obj.SetValue(EnableProperty, value);

    private static void EnableChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        // Make sure this is put on a panel
        var panel = sender as Panel;
        if (panel == null)
            return;

        if (!(bool)e.NewValue)
            return;
        
        panel.Loaded -= OnPanelLoaded;
        panel.Loaded += OnPanelLoaded;

        if (panel.IsLoaded)
            OnPanelLoaded(panel, null!);
    }
    
    private static void OnPanelLoaded(object sender, RoutedEventArgs e) => SetMarginsOnPanelLoaded(sender, null!);

    private static void MarginChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Make sure this is put on a panel
        var panel = sender as Panel;
        if (panel == null)
            return;

        if (panel.IsLoaded)
            SetMarginsOnPanelLoaded(panel, null!);
    }

    private static void SetMarginsOnPanelLoaded(object sender, RoutedEventArgs e)
    {
        var panel = (Panel)sender;

        // Go over the children and set margin for them:
        for (var x = 0; x < panel.Children.Count; x++)
        {
            // Get child element.
            UIElement child = panel.Children[x];
            var fe = child as FrameworkElement;
            if (fe == null)
                continue;

            // Check if there's any visible future element.
            bool noVisibleAfter = true;
            for (int y = x + 1; y < panel.Children.Count; y++)
            {
                UIElement nextChild = panel.Children[y];
                var feNext = nextChild as FrameworkElement;
                if (feNext == null)
                    continue;

                if (feNext.Visibility != Visibility.Visible) 
                    continue;

                noVisibleAfter = false;
                break;
            }

            bool isLastItem = x == panel.Children.Count - 1 || noVisibleAfter;
            fe.Margin = isLastItem ? GetLastItemMargin(panel) : GetMargin(panel);
        }
    }

    // Using a DependencyProperty as the backing store for Margin. This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(MarginSetter),
            new UIPropertyMetadata(false, EnableChanged));

    public static readonly DependencyProperty MarginProperty =
        DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter),
            new UIPropertyMetadata(new Thickness(), MarginChanged));

    public static readonly DependencyProperty LastItemMarginProperty =
        DependencyProperty.RegisterAttached("LastItemMargin", typeof(Thickness), typeof(MarginSetter),
            new UIPropertyMetadata(new Thickness(), MarginChanged));
}