using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Keyboards.Converters
{
    public class LayoutCodeToKeyboardLayoutConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resourceName = string.Empty;

            //keyboard button change cuture
            if (value.ToString().Contains(".en-EN"))
            {
                var currentCulture = CultureInfo.GetCultureInfo("en-EN");
                var type = value.ToString().Replace(".en-EN", "");

                Localized.Instance.LastKeyboardCulture = Localized.Instance.CurrentKeyboardCulture;
                Localized.Instance.CurrentKeyboardCulture = currentCulture;
                resourceName = string.Concat(currentCulture.TwoLetterISOLanguageName, "-", type, ".json").ToLowerInvariant();
            }
            //keyboard button secondary command turn at the original keyboard culture
            else if (Localized.Instance.LastKeyboardCulture != null &&
                (value.ToString() == "Lowercase." + Localized.Instance.LastKeyboardCulture.Name ||
                value.ToString() == "Uppercase." + Localized.Instance.LastKeyboardCulture.Name))
            {
                var type = value.ToString().Replace("." + Localized.Instance.LastKeyboardCulture.Name, "");

                Localized.Instance.CurrentKeyboardCulture = Localized.Instance.LastKeyboardCulture;
                Localized.Instance.LastKeyboardCulture = null;
                resourceName = string.Concat(Localized.Instance.CurrentKeyboardCulture.TwoLetterISOLanguageName, "-", type, ".json").ToLowerInvariant();
            }
            else
            {
                var currentCulture = Localized.Instance.CurrentKeyboardCulture;
                resourceName = string.Concat(currentCulture.TwoLetterISOLanguageName, "-", value, ".json").ToLowerInvariant();
            }

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
