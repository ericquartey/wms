using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        #region Properties

        public SolidColorBrush FalseColor { get; set; }

        public SolidColorBrush TrueColor { get; set; }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region Validation

            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException(nameof(targetType));
            }

            if (!(this.TrueColor is SolidColorBrush))
            {
                throw new InvalidOperationException(nameof(this.TrueColor));
            }

            if (!(this.FalseColor is SolidColorBrush))
            {
                throw new InvalidOperationException(nameof(this.FalseColor));
            }

            if (!(value is bool condition))
            {
                throw new InvalidOperationException(nameof(value));
            }

            #endregion

            return condition ? this.TrueColor : this.FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
