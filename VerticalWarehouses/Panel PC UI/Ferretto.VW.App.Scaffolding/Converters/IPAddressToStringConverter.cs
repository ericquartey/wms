using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class IPAddressToStringConverter : IValueConverter
    {
        private byte[] _ipV4;
        private Models.ScaffoldedEntity _entity;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this._entity = null;
            this._ipV4 = new byte[4];

            if (value is System.Net.IPAddress ipAddress)
            {
                byte[] bytes = this._ipV4 = ipAddress.GetAddressBytes();
                if (parameter != null
                    && int.TryParse(System.Convert.ToString(parameter, CultureInfo.InvariantCulture), out int index)
                    && index >= 0
                    && index <= 3)
                {
                    return System.Convert.ChangeType(bytes[index], targetType, culture);
                }

            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && int.TryParse(System.Convert.ToString(parameter, CultureInfo.InvariantCulture), out int index) && index >= 0 && index <= 3)
            {
                if (byte.TryParse(System.Convert.ToString(value, culture), NumberStyles.Integer, culture, out byte b))
                {
                    this._ipV4[index] = b;
                    return new System.Net.IPAddress(this._ipV4);
                }
            }

            return null;
        }
    }
}
