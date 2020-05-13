using System;
using System.Globalization;
using System.Windows.Data;

namespace Ferretto.VW.InvertersParametersGenerator
{
    public class MultiBooleanToUiConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string logicalOperaror = System.Convert.ToString(parameter, culture) ?? "And";
            bool isLogicOr = string.Equals(logicalOperaror, "Or", StringComparison.OrdinalIgnoreCase);

            // choose the value that better fits the least-probable result:
            // 'Or' means: 'at least 1 true' (start with false and check for a true that breaks the loop)
            // 'And' means: 'Not even 1 false' (start with true and check for a false that breaks the loop)
            bool finalValue = isLogicOr ? false : true;
            if (values != null)
            {
                foreach (var value in values)
                {
                    if (isLogicOr)
                    {
                        if (value.Equals(true))
                        {
                            finalValue = true;
                            break;
                        }
                    }
                    else if (value.Equals(false))
                    {
                        finalValue = false;
                        break;
                    }
                }
            }

            return TruthyAssertor.Convert(finalValue, targetType, parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
