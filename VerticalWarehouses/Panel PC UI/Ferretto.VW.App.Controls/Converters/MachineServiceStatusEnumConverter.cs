﻿using System;
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
            switch (value)
            {
                case MachineServiceStatus.Expired:
                    return Resources.OperatorApp.MachineServiceStatus_Expired;

                case MachineServiceStatus.Expiring:
                    return Resources.OperatorApp.MachineServiceStatus_Expiring;

                case MachineServiceStatus.Valid:
                    return Resources.OperatorApp.MachineServiceStatus_Valid;

               default:
                    return Resources.OperatorApp.MachineServiceStatus_Unknown;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
