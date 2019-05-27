using System;
using System.Linq;
using System.Windows.Data;

namespace Ferretto.WMS.App.Controls
{
    public class LogicalOrConverter : IMultiValueConverter
    {
        #region Properties

        public IValueConverter FinalConverter { get; set; }

        #endregion

        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var conditionResult = values.Any(v => (bool)v);

            return this.FinalConverter == null ? conditionResult : this.FinalConverter.Convert(conditionResult, targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
