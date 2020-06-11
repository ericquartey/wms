using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.Converters
{
    internal sealed class InProgressToVisibilityConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                throw new ArgumentException($"This converter suports {typeof(StepStatus)} source type.", nameof(values));
            }

            if (!Enum.IsDefined(typeof(StepStatus), values[0]))
            {
                throw new ArgumentException($"This converter suports {typeof(StepStatus)} source type.", nameof(values));
            }

            if (values[1] is int?)
            {
                var stepStatus = (StepStatus)values[0];

                var progressPercentage = (int?)values[1];
                return stepStatus is StepStatus.InProgress && progressPercentage.HasValue
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
