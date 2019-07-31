﻿using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumItemManagementTypeConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case ItemManagementType.FIFO:
                    return Resources.OperatorApp.ItemManagementType_FIFO;

                case ItemManagementType.Volume:
                    return Resources.OperatorApp.ItemManagementType_Volume;

                case ItemManagementType.NotSpecified:
                    return Resources.OperatorApp.ItemManagementType_NotSpecified;
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
