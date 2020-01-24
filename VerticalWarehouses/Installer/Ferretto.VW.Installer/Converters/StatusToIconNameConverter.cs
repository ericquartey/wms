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
                case StepStatus.Done: return "CheckCircle";
                case StepStatus.Failed: return "AlertCircle";
                case StepStatus.ToDo: return "Clock";
                case StepStatus.InProgress: return "PlayCircle";
                case StepStatus.RollingBack: return "ArrowLeftDropCircle";
                case StepStatus.RolledBack: return "SkipPreviousCircle";
                case StepStatus.RollbackFailed: return "CloseCircle";
                default: return "CircleOutline";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
