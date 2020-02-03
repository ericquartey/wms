using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Ferretto.VW.App.Controls.Controls.Keyboards;

namespace Ferretto.VW.App.Scaffolding.Converters
{
    public class KeyboardTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(KeyboardType))
            {
                throw new ArgumentException($"Type {targetType} is not the expected one.", nameof(targetType));
            }

            KeyboardType? keyboard = null;
            if (value is Models.ScaffoldedEntity entity)
            {
                var metadata = entity.Metadata;


                UIHintAttribute uiHint = metadata.OfType<UIHintAttribute>().FirstOrDefault();
                if (uiHint != null)
                {
                    // TODO: custom stuff
                }

                // try with datatype
                if (!keyboard.HasValue)
                {
                    DataTypeAttribute dataType = metadata.OfType<DataTypeAttribute>().FirstOrDefault();
                    if (dataType != null)
                    {
                        switch (dataType.DataType)
                        {
                            case DataType.CreditCard:
                                keyboard = KeyboardType.Numpad;
                                break;
                        }
                    }
                }

                // try with very type
                if (!keyboard.HasValue)
                {
                    Type type = entity.Property.PropertyType;
                    if (type == typeof(System.Net.IPAddress))
                    {
                        keyboard = KeyboardType.Numpad;
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
                            keyboard = KeyboardType.Numpad;
                        }
                    }
                }

            }
            return keyboard ?? KeyboardType.QWERTY;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
