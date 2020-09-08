using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class MachinesToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is IEnumerable<MachinePick> pickMachines)
                {
                    return string.Join(", ", pickMachines.Select(m => m.Nickname).ToArray());
                }

                if (value is IEnumerable<Machine> machines)
                {
                    return string.Join(", ", machines.Select(m => m.Id).ToArray());
                }

                if (value is IEnumerable<Machine2> listRowMachines)
                {
                    return string.Join(", ", listRowMachines.Select(m => m.Id).ToArray());
                }

                return null;
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
