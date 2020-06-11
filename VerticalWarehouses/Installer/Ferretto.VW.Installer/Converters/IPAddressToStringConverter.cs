using System;
using System.Globalization;
using System.Windows.Data;

namespace Ferretto.VW.Installer.Converters
{
    public class IPAddressToStringConverter : IValueConverter
    {
        #region Fields

        private byte[] _ipV4;

        #endregion

        #region Methods

        public object Convert/*ToString*/(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this._ipV4 = new byte[4];

            if (value is System.Net.IPAddress ipAddress)
            {
                var bytes = this._ipV4 = ipAddress.GetAddressBytes();
                if (parameter != null
                    && int.TryParse(System.Convert.ToString(parameter, CultureInfo.InvariantCulture), out var index)
                    && index >= 0
                    && index <= 3)
                {
                    return System.Convert.ChangeType(bytes[index], targetType, culture);
                }

                return string.Join(".", bytes);
            }
            return value;
        }

        public object ConvertBack/*FromString*/(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && int.TryParse(System.Convert.ToString(parameter, CultureInfo.InvariantCulture), out var index) && index >= 0 && index <= 3)
            {
                if (byte.TryParse(System.Convert.ToString(value, culture), NumberStyles.Integer, culture, out var b))
                {
                    this._ipV4[index] = b;
                }
                return new System.Net.IPAddress(this._ipV4);
            }

            return null;
        }

        #endregion
    }
}
