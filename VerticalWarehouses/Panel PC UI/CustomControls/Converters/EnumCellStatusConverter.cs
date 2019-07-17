using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumCellStatusConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case Status.Free:
                    return Resources.OperatorApp.Cell_Status_Free;

                case Status.Disabled:
                    return Resources.OperatorApp.Cell_Status_Disabled;

                case Status.Occupied:
                    return Resources.OperatorApp.Cell_Status_Occupied;

                case Status.Unusable:
                    return Resources.OperatorApp.Cell_Status_Unusable;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
