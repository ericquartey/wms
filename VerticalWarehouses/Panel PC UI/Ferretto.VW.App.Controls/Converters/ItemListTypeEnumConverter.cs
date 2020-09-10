using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ItemListTypeEnumConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case ItemListType.Inventory:
                        return Resources.Localized.Get("OperatorApp.ItemListType_Inventory");

                    case ItemListType.Pick:
                        return Resources.Localized.Get("OperatorApp.ItemListType_Pick");

                    case ItemListType.Put:
                        return Resources.Localized.Get("OperatorApp.ItemListType_Put");

                    default:
                        return Resources.Localized.Get("OperatorApp.ItemListType_NotSpecified");
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
