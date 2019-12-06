using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Modules.Layout.Converters
{
    public class NotificationSeverityToForegroundConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException();
            }

            var severity = (NotificationSeverity)value;

            var resourceKey = $"{nameof(NotificationSeverity)}Foreground{severity}";

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
