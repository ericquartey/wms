using System;
using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BoolToIconKindConverter : IValueConverter
    {
        #region Properties

        /// <summary>
        /// Gets or sets the icon kind associated to the <c>False</c> value.
        /// </summary>
        public PackIconMaterialKind FalseKind { get; set; }

        /// <summary>
        /// Gets or sets the icon kind associated to the <c>True</c> value.
        /// </summary>
        public PackIconMaterialKind TrueKind { get; set; }

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(PackIconMaterialKind))
            {
                throw new InvalidOperationException($"The target type for this converter must be of type {nameof(PackIconMaterialKind)}");
            }

            if (!(value is bool condition))
            {
                throw new InvalidOperationException($"The source type for this converter must be of type {nameof(Boolean)}");
            }

            return condition ? this.TrueKind : this.FalseKind;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
