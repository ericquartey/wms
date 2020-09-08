using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is bool bValue)
                {
                    return (bValue == true) ? Resources.Localized.Get("InstallationApp.True") : Resources.Localized.Get("InstallationApp.False");
                }

                return Resources.Localized.Get("InstallationApp.False");
            }
            catch(Exception)
            {
                return Resources.Localized.Get("InstallationApp.False");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var retValue = false;
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    switch (value.ToString().ToLower(culture))
                    {
                        case "true":
                        case "vero":
                            retValue = true;
                            break;

                        case "false":
                        case "falso":
                            retValue = false;
                            break;

                        default:
                            retValue = false;
                            break;
                    }
                }

                return retValue;
            }
            catch(Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
