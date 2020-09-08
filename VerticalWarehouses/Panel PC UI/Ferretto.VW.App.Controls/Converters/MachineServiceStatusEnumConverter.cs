using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class MachineServiceStatusEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case MachineServiceStatus.Expired:
                        return Resources.Localized.Get("OperatorApp.MachineServiceStatus_Expired");

                    case MachineServiceStatus.Expiring:
                        return Resources.Localized.Get("OperatorApp.MachineServiceStatus_Expiring");

                    case MachineServiceStatus.Valid:
                        return Resources.Localized.Get("OperatorApp.MachineServiceStatus_Valid");

                    case MachineServiceStatus.Completed:
                        return Resources.Localized.Get("OperatorApp.MachineServiceStatus_Completed");

                    default:
                        return Resources.Localized.Get("OperatorApp.MachineServiceStatus_Unknown");
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
