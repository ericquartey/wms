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
    [ValueConversion(typeof(string), typeof(string))]
    public class CounterNameConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case "TotalLoadUnitsInBay1":
                        return Resources.Localized.Get("InstallationApp.TotalLoadUnitsInBay1");

                    case "TotalLoadUnitsInBay2":
                        return Resources.Localized.Get("InstallationApp.TotalLoadUnitsInBay2");

                    case "TotalLoadUnitsInBay3":
                        return Resources.Localized.Get("InstallationApp.TotalLoadUnitsInBay3");

                    case "TotalMissions":
                        return Resources.Localized.Get("InstallationApp.TotalMissions");

                    default: //None
                        return Resources.Localized.Get("InstallationApp.None");
                }
            }
            catch(Exception)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
