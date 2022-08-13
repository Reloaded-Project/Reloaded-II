namespace Reloaded.Mod.Launcher.Controls.Selectors;

/// <summary>
/// Fixed selector for datatemplates, to accelerate finding of templates for ItemsControl.
/// Might be helpful https://nicksnettravels.builttoroam.com/xaml-basics-datatemplateselector/
/// </summary>
public class FixedTemplateSelector : DataTemplateSelector
{
    public DataTemplate Template
    {
        get => _template;
        set => _template = value;
    }

    private DataTemplate _template;

    public override DataTemplate SelectTemplate(object item, DependencyObject container) => Template;
}