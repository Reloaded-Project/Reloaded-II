using Button = Sewer56.UI.Controller.Core.Enums.Button;
using ILocalizationProvider = Sewer56.UI.Controller.ReloadedInput.Configurator.ILocalizationProvider;

namespace Reloaded.Mod.Launcher.Utility;

internal class ReloadedInputLocalizationProvider : ILocalizationProvider
{
    public XamlResourceProvider _xamlResourceProvider = new ();

    public string? GetName(Button button)
    {
        return _xamlResourceProvider.Get<string>($"ConfiguratorBtn{button.ToStringFast()}Name");
    }

    public string? GetDescription(Button button)
    {
        return _xamlResourceProvider.Get<string>($"ConfiguratorBtn{button.ToStringFast()}Description");
    }

    public string? GetName(CustomStickMappingEntry entry)
    {
        return _xamlResourceProvider.Get<string>($"ConfiguratorStick{entry}Name");
    }

    public string? GetDescription(CustomStickMappingEntry entry)
    {
        return _xamlResourceProvider.Get<string>($"ConfiguratorStick{entry}Description");
    }

    public string? GetCustomString(CustomStrings customString)
    {
        return _xamlResourceProvider.Get<string>($"ConfiguratorCustomString{customString}");
    }

    public string? GetText(Input.Configurator.Localization.CustomStrings text)
    {
        return _xamlResourceProvider.Get<string>($"ConfiguratorLibCustomString{text}");
    }
}