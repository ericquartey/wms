using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Controls.Converters
{
    public class NotificationSeverityToIconNameConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(MahApps.Metro.IconPacks.PackIconModernKind))
            {
                throw new InvalidOperationException();
            }

            var severity = (NotificationSeverity)value;

            if (severity == NotificationSeverity.PtlInfo)
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

            switch (severity)
            {
                case NotificationSeverity.Error:
                    return MahApps.Metro.IconPacks.PackIconModernKind.WarningCircle;

                case NotificationSeverity.Low:
                case NotificationSeverity.Info:
                    return MahApps.Metro.IconPacks.PackIconModernKind.InformationCircle;

                case NotificationSeverity.Warning:
                    return MahApps.Metro.IconPacks.PackIconModernKind.Warning;

                case NotificationSeverity.Success:
                    return MahApps.Metro.IconPacks.PackIconModernKind.Check;

                default:
                    return MahApps.Metro.IconPacks.PackIconModernKind.None;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
