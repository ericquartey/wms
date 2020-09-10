using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BayNumberToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case BayNumber.BayOne:
                        return Resources.Localized.Get("InstallationApp.Bay1");

                    case BayNumber.BayTwo:
                        return Resources.Localized.Get("InstallationApp.Bay2");

                    case BayNumber.BayThree:
                        return Resources.Localized.Get("InstallationApp.Bay1");

                    case BayNumber.ElevatorBay:
                        return Resources.Localized.Get("InstallationApp.ElevatorBay");

                    case BayNumber.All:
                        return Resources.Localized.Get("InstallationApp.ElevatorBay");

                    default: //None
                        return Resources.Localized.Get("InstallationApp.None");
                }
            }
            catch(Exception)
            {
                return "";
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
