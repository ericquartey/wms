using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class InverterToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case InverterIndex.MainInverter:
                        return InverterIndex.MainInverter.ToString();

                    case InverterIndex.Slave1:
                        return InverterIndex.Slave1.ToString();

                    case InverterIndex.Slave2:
                        return InverterIndex.Slave2.ToString();

                    case InverterIndex.Slave3:
                        return InverterIndex.Slave3.ToString();

                    case InverterIndex.Slave4:
                        return InverterIndex.Slave4.ToString();

                    case InverterIndex.Slave5:
                        return InverterIndex.Slave5.ToString();

                    case InverterIndex.Slave6:
                        return InverterIndex.Slave6.ToString();

                    case InverterIndex.Slave7:
                        return InverterIndex.Slave7.ToString();

                    case InverterIndex.None:
                        return Resources.Localized.Get("OperatorApp.BlockLevelNone");

                    case InverterIndex.All:
                    default:
                        return Resources.Localized.Get("OperatorApp.BayNumberAll");
                }
            }
            catch (Exception)
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
