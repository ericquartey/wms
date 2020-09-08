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
            try
            {
                if (targetType == typeof(FontFamily))
                {
                    if (TruthyAssertor.IsTruthy(value))
                    {
                        var assembly = Assembly.GetAssembly(this.GetType());
                        var assemblyName = assembly.GetName().Name;
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
