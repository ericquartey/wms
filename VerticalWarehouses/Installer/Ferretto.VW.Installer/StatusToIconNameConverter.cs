using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.Installer
{
    internal sealed class StatusToIconNameConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((StepStatus)value)
            {
                case StepStatus.Done: return "CircleOutline";
                case StepStatus.Failed: return "CloseCircleOutline";
                case StepStatus.ToDo: return "CloseCircleOutline";
                case StepStatus.InProgress: return "ArrowRightDropCircleOutline";
                case StepStatus.RollingBack: return "BackupRestore";
                case StepStatus.RollbackFailed: return "BackupRestore";
                default: return "ABTesting";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
