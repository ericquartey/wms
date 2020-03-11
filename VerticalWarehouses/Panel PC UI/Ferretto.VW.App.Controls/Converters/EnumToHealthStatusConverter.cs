using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumToHealthStatusConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case HealthStatus.Healthy:
                    return Resources.InstallationApp.WmsOn;

                case HealthStatus.Unknown:
                case HealthStatus.Unhealthy:
                case HealthStatus.Degraded:
                case HealthStatus.Initialized:
                case HealthStatus.Initializing:
                    return Resources.InstallationApp.WmsOff;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
