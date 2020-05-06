using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.App.Controls.Converters
{
    public class AlphaNumericBarSizeEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case AlphaNumericBarSize.ExtraLarge:
                    return Resources.InstallationApp.AlphaNumericBarSizeXL;

                case AlphaNumericBarSize.ExtraSmall:
                    return Resources.InstallationApp.AlphaNumericBarSizeXS;

                case AlphaNumericBarSize.Large:
                    return Resources.InstallationApp.AlphaNumericBarSizeL;

                case AlphaNumericBarSize.Medium:
                    return Resources.InstallationApp.AlphaNumericBarSizeM;

                default:
                    return Resources.InstallationApp.AlphaNumericBarSizeS;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
