using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.Installer
{
    internal sealed class StatusToColorConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(StepStatus), value))
            {
                throw new ArgumentException(nameof(value));
            }

            switch ((StepStatus)value)
            {
                case StepStatus.Done: return Color.DarkGreen;
                case StepStatus.Failed: return Brushes.Firebrick;
                case StepStatus.ToDo: return Brushes.Gray;
                case StepStatus.InProgress: return Brushes.Blue;
                case StepStatus.RollingBack: return Brushes.Gold;
                case StepStatus.RollbackFailed: return Brushes.Firebrick;
                default: return Brushes.Pink;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
