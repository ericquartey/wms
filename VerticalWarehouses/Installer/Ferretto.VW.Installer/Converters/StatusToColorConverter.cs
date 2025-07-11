﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.Converters
{
    internal sealed class StatusToColorConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.IsDefined(typeof(StepStatus), value))
            {
                throw new ArgumentException($"The converter accepts only values of type {nameof(StepStatus)}", nameof(value));
            }

            switch ((StepStatus)value)
            {
                case StepStatus.Done: return "#008b00";
                case StepStatus.Failed: return "Firebrick";
                case StepStatus.ToDo: return "Gray";
                case StepStatus.InProgress: return nameof(Colors.CornflowerBlue);
                case StepStatus.RolledBack: return "Gold";
                case StepStatus.RollingBack: return "Gold";
                case StepStatus.RollbackFailed: return "Firebrick";
                default: return nameof(Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
