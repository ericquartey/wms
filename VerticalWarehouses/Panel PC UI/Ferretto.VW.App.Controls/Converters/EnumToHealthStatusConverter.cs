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
            try
            {
                switch (value)
                {
                    case HealthStatus.Healthy:
                        return Resources.Localized.Get("InstallationApp.WmsOn");

                    case HealthStatus.Unknown:
                    case HealthStatus.Unhealthy:
                    case HealthStatus.Degraded:
                    case HealthStatus.Initialized:
                    case HealthStatus.Initializing:
                        return Resources.Localized.Get("InstallationApp.WmsOff");
                }

                return string.Empty;
            }
            catch(Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
