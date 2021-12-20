using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Keyboards.Converters
{
    public class KeyboardCommandToContentConverter : IValueConverter
    {
        #region Fields

        private const string IconPattern = @"^\{icon:(?<Kind>[\w]+)(:(?<Rotation>[\d]{1,3}))?\}$";

        #endregion

        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is KeyboardKeyCommand command)
            {
                if (!string.IsNullOrEmpty(command.Caption))
                {
                    //check the SecondaryCommandText to change button text
                    if (command.SecondaryCommandText != null &&
                        Localized.Instance.LastKeyboardCulture != null &&
                        (command.SecondaryCommandText.ToLowerInvariant().Contains("uppercase") || command.SecondaryCommandText.ToLowerInvariant().Contains("lowercase")))
                    {
                        return Localized.Instance.LastKeyboardCulture.ThreeLetterISOLanguageName.ToUpperInvariant();
                    }

                    var match = Regex.Match(command.Caption, IconPattern);
                    if (match?.Success == true)
                    {
                        var kindString = match.Groups["Kind"].Value;
                        if (Enum.TryParse<MahApps.Metro.IconPacks.PackIconFontAwesomeKind>(kindString, out var kind))
                        {
                            var rotation = 0D;
                            var rotationStr = match.Groups["Rotation"]?.Value;
                            if (!string.IsNullOrEmpty(rotationStr))
                            {
                                rotation = double.Parse(rotationStr);
                            }
                            return new MahApps.Metro.IconPacks.PackIconFontAwesome
                            {
                                Kind = kind,
                                RotationAngle = rotation
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
