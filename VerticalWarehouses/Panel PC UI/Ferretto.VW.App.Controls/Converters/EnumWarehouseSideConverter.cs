using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumWarehouseSideConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case WarehouseSide.Back:
                        return Resources.Localized.Get("General.WarehouseSide_Back");

                    case WarehouseSide.Front:
                        return Resources.Localized.Get("General.WarehouseSide_Front");
                }
            }
            catch(Exception)
            {
                return value;
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
