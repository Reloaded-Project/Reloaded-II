using System.Collections.Generic;
using System.Windows;
using Reloaded.Mod.Launcher.Lib.Interop;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Interop;

/// <summary>
/// Represents an individual XAML resource sourced from a WPF application.
/// </summary>
public class XamlResourceEx<TGeneric> : XamlResource<TGeneric>, IDictionaryResource<TGeneric>
{
    public XamlResourceEx(object resourceKey) : base(resourceKey) { }

    public XamlResourceEx(object resourceKey, IEnumerable<FrameworkElement> additionalSources) : base(resourceKey, additionalSources) { }

    public XamlResourceEx(object resourceKey, IEnumerable<FrameworkElement> additionalSources, FrameworkElement biasedElement) : base(resourceKey, additionalSources, biasedElement) { }
    
    public bool TrySet(TGeneric value) => base.Set(value);
}