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
            this._ipV4 = null;

            if (value is Models.ScaffoldedEntity entity)
            {
                this._entity = entity;
                value = entity.Property.GetValue(entity.Instance);
            }

            if (value is System.Net.IPAddress ipAddress)
            {
                byte[] bytes = this._ipV4 = ipAddress.GetAddressBytes();
                if (parameter == null || !int.TryParse(System.Convert.ToString(parameter), out int index) || index < 0 || index > 3)
                {
                    return string.Format($"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}");
                }

                return bytes[index].ToString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (this._ipV4 != null)
            {
                System.Net.IPAddress ipAddress;
                if (parameter != null && int.TryParse(System.Convert.ToString(parameter), out int index) && index >= 0 && index <= 3)
                {
                    this._ipV4[index] = System.Convert.ToByte(value);
                    ipAddress = new System.Net.IPAddress(this._ipV4);
                }
                else
                {
                    ipAddress = System.Net.IPAddress.Parse(System.Convert.ToString(value));
                }

                if (this._entity != null)
                {
                    this._entity.Property.SetValue(this._entity.Instance, ipAddress);
                    return this._entity;
                }
                return ipAddress;
            }

            return value;
        }
    }
}
