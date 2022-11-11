using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Controls.Converters
{
    public class NotificationSeverityToBrushConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException();
            }

            var severity = (NotificationSeverity)value;

            if (severity == NotificationSeverity.PtlInfo || severity == NotificationSeverity.PtlInfoStart)
            {
                severity = NotificationSeverity.Info;
            }
            else if (severity == NotificationSeverity.PtlSuccess)
            {
                severity = NotificationSeverity.Success;
            }
            else if (severity == NotificationSeverity.PtlWarning)
            {
                severity = NotificationSeverity.Warning;
            }
            else if (severity == NotificationSeverity.PtlError)
            {
                severity = NotificationSeverity.Error;
            }

            var resourceKey = $"{nameof(NotificationSeverity)}{severity}";

            if (Application.Current.Resources.Contains(resourceKey))
            {
                return Application.Current.Resources[resourceKey];
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
