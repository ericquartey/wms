using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ferretto.VW.App.Keyboards.Converters
{
    public class KeyboardCommandToContentConverter : IValueConverter
    {
        #region Fields

        private const string IconPattern = @"^\{icon:(?<Kind>[\w]+)\}$";

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Ferretto.VW.App.Keyboards.KeyboardKeyCommand command)
            {
                if (!string.IsNullOrEmpty(command.Caption))
                {
                    var match = Regex.Match(command.Caption, IconPattern);
                    if (match?.Success == true)
                    {
                        string kindString = match.Groups["Kind"].Value;
                        if (Enum.TryParse<MahApps.Metro.IconPacks.PackIconFontAwesomeKind>(kindString, out var kind))
                        {
                            return new MahApps.Metro.IconPacks.PackIconFontAwesome
                            {
                                Kind = kind
                            };
                        }
                        return kindString;
                    }
                    return command.Caption;
                }
                return command.CommandText;
            }

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
