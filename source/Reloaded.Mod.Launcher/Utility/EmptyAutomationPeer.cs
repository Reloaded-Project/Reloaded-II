namespace Reloaded.Mod.Launcher.Utility;

/// <summary>
/// Returns an empty automation peer that returns nothing for performance.
/// </summary>
public class EmptyAutomationPeer : FrameworkElementAutomationPeer
{
    public EmptyAutomationPeer(FrameworkElement owner) : base(owner) { }

    protected override string GetNameCore() => "CustomWindowAutomationPeer";

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Window;

    protected override List<AutomationPeer> GetChildrenCore() => new();
}