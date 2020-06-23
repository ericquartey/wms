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
    public class AxisToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case Axis.BayChain:
                    return Resources.Localized.Get("InstallationApp.BayChain");

                case Axis.Horizontal:
                    return Resources.Localized.Get("InstallationApp.Horizontal");

                case Axis.HorizontalAndVertical:
                    return Resources.Localized.Get("InstallationApp.HorizontalAndVertical");

                case Axis.Vertical:
                    return Resources.Localized.Get("InstallationApp.Vertical");

                default: //None
                    return Resources.Localized.Get("InstallationApp.None");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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

        #endregion
    }
}
