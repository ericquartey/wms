using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumItemManagementTypeConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case ItemManagementType.FIFO:
                        return Resources.Localized.Get("OperatorApp.ItemManagementType_FIFO");

                    case ItemManagementType.Volume:
                        return Resources.Localized.Get("OperatorApp.ItemManagementType_Volume");

                    case ItemManagementType.NotSpecified:
                        return Resources.Localized.Get("OperatorApp.ItemManagementType_NotSpecified");
                }

                return string.Empty;
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
