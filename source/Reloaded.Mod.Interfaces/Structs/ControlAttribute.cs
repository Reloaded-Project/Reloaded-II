using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces.Structs;

public enum SliderControlTickPlacement
{
    None = 0,
    TopLeft = 1,
    BottomRight = 2,
    Both = 3,
}

public interface ICustomControlAttribute { }

[System.AttributeUsage(System.AttributeTargets.Property)]
public class SliderControlParamsAttribute : Attribute, ICustomControlAttribute
{
    public double Minimum { get; }
    public double Maximum { get; }
    public double SmallChange { get; }
    public double LargeChange { get; }
    public int TickFrequency { get; }
    public bool IsSnapToTickEnabled { get; }
    public SliderControlTickPlacement TickPlacement { get; }
    
    public SliderControlParamsAttribute(
        double minimum = 0.0,
        double maximum = 1.0,
        double smallChange = 0.1,
        double largeChange = 1.0,
        int tickFrequency = 10,
        bool isSnapToTickEnabled = false,
        SliderControlTickPlacement tickPlacement = SliderControlTickPlacement.None
    ) {
        Minimum = minimum;
        Maximum = maximum;
        SmallChange = smallChange;
        LargeChange = largeChange;
        TickFrequency = tickFrequency;
        IsSnapToTickEnabled = isSnapToTickEnabled;
        TickPlacement = tickPlacement;
    }
}
