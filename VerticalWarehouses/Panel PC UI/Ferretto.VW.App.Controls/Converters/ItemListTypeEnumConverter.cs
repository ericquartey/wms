﻿using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ItemListTypeEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case ItemListType.Inventory:
                    return Resources.OperatorApp.ItemListType_Inventory;

                case ItemListType.Pick:
                    return Resources.OperatorApp.ItemListType_Pick;

                case ItemListType.Put:
                    return Resources.OperatorApp.ItemListType_Put;

                default:
                    return Resources.OperatorApp.ItemListType_NotSpecified;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
