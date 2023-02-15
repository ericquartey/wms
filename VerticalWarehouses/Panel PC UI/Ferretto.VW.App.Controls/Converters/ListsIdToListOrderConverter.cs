using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Controls.Converters
{
    public class ListsIdToListOrderConverter : DependencyObject, IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var id = value?.ToString();

            if (id == null)
            {
                return "";
            }

            return id.StartsWith("REF") && id.EndsWith("accorp") ? Resources.Localized.Get("OperatorApp.ListOrderByLight") : Resources.Localized.Get("OperatorApp.ListOrderByNormal");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
