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

        /// <summary>
        /// Gets or sets the brush associated to the <c>False</c> value.
        /// </summary>
        public Brush FalseBrush { get; set; }

        /// <summary>
        /// Gets or sets the name of the brush resource associated to the <c>False</c> value.
        /// </summary>
        /// <remarks>
        /// The value is ignored it the <see cref="FalseBrush"/> property is specified.</remarks>
        public string FalseBrushResourceName { get; set; }

        /// <summary>
        /// Gets or sets the brush associated to the <c>True</c> value.
        /// </summary>
        public Brush TrueBrush { get; set; }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException($"The target type for this converter must be of type {nameof(Brush)}");
            }

            if (!(value is bool condition))
            {
                throw new InvalidOperationException($"The source type for this converter must be of type {nameof(Boolean)}");
            }

            var falseBrush = this.FalseBrush;
            if (falseBrush is null)
            {
                falseBrush = Application.Current.FindResource(this.FalseBrushResourceName) as Brush;
                if (falseBrush is null)
                {
                    throw new InvalidOperationException($"The resource '{this.FalseBrushResourceName}' is not of type {nameof(Brush)}.");
                }
            }

            return condition ? this.TrueBrush : falseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
