using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Converters
{
    public class BooleanToPasswordFontFamilyConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(FontFamily))
            {
                if (TruthyAssertor.IsTruthy(value))
                {
                    Assembly assembly = Assembly.GetAssembly(this.GetType());
                    string assemblyName = assembly.GetName().Name;
                    //Uri baseUri = new Uri($"pack://application:,,,/{ assemblyName };component/Fonts/");
                    return new FontFamily(new Uri("pack://application:,,,/"), $"./{assemblyName};component/Fonts/#Password");
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
