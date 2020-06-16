using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.Converters
{
    internal sealed class StatusToSpinConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(StepStatus), value))
            {
                throw new ArgumentException($"The converter accepts only values of type {nameof(StepStatus)}", nameof(value));
            }

            return value is StepStatus.InProgress;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
