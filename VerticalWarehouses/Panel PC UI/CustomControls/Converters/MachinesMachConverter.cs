﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class MachinesMatchConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Machine> machines &&
                parameter is int machineId)
            {
                return machines.Any(m => m.Id == machineId);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
