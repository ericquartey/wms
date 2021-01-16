using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Keyboards.Converters
{
    public class LayoutCodeToKeyboardLayoutConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resourceName = string.Concat(culture.TwoLetterISOLanguageName, "-", value, ".json").ToLowerInvariant();
            var assembly = Assembly.GetAssembly(this.GetType());
            var assemblyName = assembly.GetName().Name;
            var jsonUri = new Uri(string.Concat($"pack://application:,,,/{ assemblyName };component/Resources/", resourceName));
            try
            {
                var sri = Application.GetResourceStream(jsonUri);
                using (var stream = new StreamReader(sri.Stream))
                {
                    var json = stream.ReadToEnd();
                    return KeyboardLayout.FromJson(json);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
