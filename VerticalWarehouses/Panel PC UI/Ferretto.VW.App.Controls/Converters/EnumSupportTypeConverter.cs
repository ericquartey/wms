using System;
using System.Globalization;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class EnumSupportTypeConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                switch (value)
                {
                    case SupportType.Insert:
                        return Resources.Localized.Get("OperatorApp.SupportType_Insert");

                    case SupportType.Above:
                        return Resources.Localized.Get("OperatorApp.SupportType_Above");

                    default:
                        return string.Empty;
                }
            }
            catch (Exception)
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
