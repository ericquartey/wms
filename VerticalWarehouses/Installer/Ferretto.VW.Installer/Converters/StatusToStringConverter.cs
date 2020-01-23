using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.Installer
{
    internal sealed class StatusToStringConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((StepStatus)value)
            {
                case StepStatus.Done: return "Completato";
                case StepStatus.Failed: return "Fallito";
                case StepStatus.ToDo: return "In Attesa";
                case StepStatus.InProgress: return "In Esecuzione";
                case StepStatus.RollingBack: return "Ripristino in Corso";
                case StepStatus.RolledBack: return "Ripristinato";
                case StepStatus.RollbackFailed: return "Ripristino fallito";
                default: return "Non definito";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
