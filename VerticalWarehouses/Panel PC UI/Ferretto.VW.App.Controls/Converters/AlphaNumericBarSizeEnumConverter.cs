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
            try
            {
                AlphaNumericBarSize size = AlphaNumericBarSize.Small;
                if (value != null)
                {
                    Enum.TryParse(value.ToString(), out size);
                }
                switch (size)
                {
                    case AlphaNumericBarSize.ExtraLarge:
                        return Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeXL");

                    case AlphaNumericBarSize.ExtraSmall:
                        return Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeXS");

                    case AlphaNumericBarSize.ExtraExtraSmall:
                        return Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeXXS");

                    case AlphaNumericBarSize.Large:
                        return Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeL");

                    case AlphaNumericBarSize.Medium:
                        return Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeM");

                    default:
                        return Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeS");
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
