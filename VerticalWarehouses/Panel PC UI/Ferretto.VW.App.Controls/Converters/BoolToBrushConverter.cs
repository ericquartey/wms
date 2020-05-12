using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        #region Properties

        public string FalseColor { get; set; }

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

            //if (!(this.FalseColor is SolidColorBrush))
            //{
            //    throw new InvalidOperationException(nameof(this.FalseColor));
            //}
            object resource = null;
            try
            {
                resource = Application.Current.FindResource(this.FalseColor);
            }
            catch (ResourceReferenceKeyNotFoundException ex)
            {
                throw new InvalidOperationException(nameof(this.FalseColor));
            }

            if (!(value is bool condition))
            {
                throw new InvalidOperationException(nameof(value));
            }

            #endregion

            return condition ? this.TrueColor : resource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
