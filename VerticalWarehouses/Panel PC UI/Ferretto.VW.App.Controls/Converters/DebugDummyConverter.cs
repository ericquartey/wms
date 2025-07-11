﻿using System;
using System.Diagnostics;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class DebugDummyConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }

        #endregion
    }
}
