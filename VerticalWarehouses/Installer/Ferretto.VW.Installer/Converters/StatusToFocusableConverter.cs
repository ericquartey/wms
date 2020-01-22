using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ferretto.VW.Installer
{
    internal sealed class StatusToFocusableConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(StepStatus), value))
            {
                throw new ArgumentException(nameof(value));
            }

            var stepStatus = (StepStatus)value;

            return stepStatus != StepStatus.ToDo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
