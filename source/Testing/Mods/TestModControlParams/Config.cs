using System.ComponentModel;
using TestModControlParams.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;

namespace TestModControlParams.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Int Slider")]
        [Description("This is a int that uses a slider control similar to a volume control slider.")]
        [DefaultValue(100)]
        [SliderControlParams(minimum: 0.0, maximum: 100.0, smallChange: 1.0, largeChange: 10.0, tickFrequency: 10, isSnapToTickEnabled: false, tickPlacement:SliderControlTickPlacement.BottomRight)]
        public int IntSlider { get; set; } = 100;

        [DisplayName("Double Slider")]
        [Description("This is a double that uses a slider control without any frills.")]
        [DefaultValue(0.5)]
        [SliderControlParams(minimum:0.0, maximum:1.0)]
        public double DoubleSlider { get; set; } = 0.5;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
