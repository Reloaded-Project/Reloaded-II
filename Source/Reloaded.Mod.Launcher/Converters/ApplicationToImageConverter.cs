
#pragma warning disable 1591

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Converters
{
    /// <summary>
    /// Converts an <see cref="IApplicationConfig"/> to an <see cref="ImageSource"/>.
    /// </summary>
    [ValueConversion(typeof(IApplicationConfig), typeof(ImageSource))]
    public class ApplicationPathToImageConverter : IValueConverter
    {
        public static ApplicationPathToImageConverter Instance = new ApplicationPathToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IApplicationConfig applicationConfig)
            {
                var viewModel = IoC.Get<AddAppViewModel>();
                var appImagePathTuple = viewModel.MainPageViewModel.Applications.First(x => x.ApplicationConfig.Equals(viewModel.Application));
                return appImagePathTuple.Image;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
