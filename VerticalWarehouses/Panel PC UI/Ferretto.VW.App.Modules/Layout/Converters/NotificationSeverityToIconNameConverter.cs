﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Modules.Layout.Converters
{
    public class NotificationSeverityToIconNameConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(MahApps.Metro.IconPacks.PackIconModernKind))
            {
                throw new InvalidOperationException();
            }

            var severity = (NotificationSeverity)value;

            switch (severity)
            {
                case NotificationSeverity.Error:
                    return MahApps.Metro.IconPacks.PackIconModernKind.WarningCircle;

                case NotificationSeverity.Info:
                    return MahApps.Metro.IconPacks.PackIconModernKind.InformationCircle;

                case NotificationSeverity.Warning:
                    return MahApps.Metro.IconPacks.PackIconModernKind.Warning;

                case NotificationSeverity.Success:
                    return MahApps.Metro.IconPacks.PackIconModernKind.Check;
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
