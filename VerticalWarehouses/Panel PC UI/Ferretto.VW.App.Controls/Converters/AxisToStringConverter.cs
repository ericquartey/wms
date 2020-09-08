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
            try
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
            catch(Exception)
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
