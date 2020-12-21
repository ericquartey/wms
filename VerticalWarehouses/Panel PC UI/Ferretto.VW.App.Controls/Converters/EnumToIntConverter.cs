using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Controls.Converters
{
    public class InverterIndexToIntConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InverterIndex inverterIndex)
            {
                return (int)inverterIndex;
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
