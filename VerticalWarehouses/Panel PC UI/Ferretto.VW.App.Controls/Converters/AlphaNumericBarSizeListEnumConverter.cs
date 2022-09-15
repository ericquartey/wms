using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class AlphaNumericBarSizeListEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                List<string> translate = new List<string>();
                foreach (var blk in values as IEnumerable<AlphaNumericBarSize>)
                {
                    switch (blk)
                    {
                        case AlphaNumericBarSize.ExtraLarge:
                            translate.Add(Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeXL"));
                            break;

                        case AlphaNumericBarSize.ExtraSmall:
                            translate.Add(Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeXS"));
                            break;

                        case AlphaNumericBarSize.ExtraExtraSmall:
                            translate.Add(Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeXXS"));
                            break;

                        case AlphaNumericBarSize.Large:
                            translate.Add(Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeL"));
                            break;

                        case AlphaNumericBarSize.Medium:
                            translate.Add(Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeM"));
                            break;

                        default:
                            translate.Add(Resources.Localized.Get("InstallationApp.AlphaNumericBarSizeS"));
                            break;
                    }
                }

                return translate;
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
