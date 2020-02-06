using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Ferretto.VW.App.Keyboards.Converters
{
    public class LayoutCodeToKeyboardLayoutConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string resourceName = string.Concat(culture.TwoLetterISOLanguageName, "-", value, ".json").ToLowerInvariant();
            Assembly assembly = Assembly.GetAssembly(this.GetType());
            string assemblyName = assembly.GetName().Name;
            Uri jsonUri = new Uri(string.Concat($"pack://application:,,,/{ assemblyName };component/Resources/", resourceName));
            try
            {
                var sri = Application.GetResourceStream(jsonUri);
                using (var stream = new StreamReader(sri.Stream))
                {
                    string json = stream.ReadToEnd();
                    return KeyboardLayout.FromJson(json);
                }
            }
            catch (Exception exc)
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
