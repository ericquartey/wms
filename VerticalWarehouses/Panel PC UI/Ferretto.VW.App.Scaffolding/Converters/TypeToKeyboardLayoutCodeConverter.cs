using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.VW.App.Controls.Controls.Keyboards;
using Ferretto.VW.App.Keyboards;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class TypeToKeyboardLayoutCodeConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ScaffoldedEntity entity)
            {
                var metadata = entity.Metadata;

                // 1. UI hints?
                UIHintAttribute uiHint = metadata.OfType<UIHintAttribute>().FirstOrDefault();
                if (uiHint != null)
                {
                    // TODO: custom stuff
                }

                // 2. data-type specified?
                DataTypeAttribute dataType = metadata.OfType<DataTypeAttribute>().FirstOrDefault();
                if (dataType != null)
                {
                    switch (dataType.DataType)
                    {
                        case DataType.PhoneNumber:
                        case DataType.CreditCard:
                            return KeyboardLayoutCodes.Numpad;
                    }
                }

                // 3. try with very type
                Type type = entity.Property.PropertyType;
                if (type == typeof(System.Net.IPAddress))
                {
                    return KeyboardLayoutCodes.Numpad;
                }
                else if (type.IsValueType)
                {
                    if (
                        type == typeof(byte)
                        || type == typeof(short)
                        || type == typeof(ushort)
                        || type == typeof(int)
                        || type == typeof(uint)
                        || type == typeof(float)
                        || type == typeof(double)
                        || type == typeof(decimal)
                        || type == typeof(long)
                        || type == typeof(ulong)
                        )
                    {
                        return KeyboardLayoutCodes.Numpad;
                    }
                }
            }
            return KeyboardLayoutCodes.Lowercase;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
