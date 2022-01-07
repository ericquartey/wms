using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.App.Keyboards.Controls;
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Keyboards
{
    public static class KeyboardKeyCommandExtensions
    {
        #region Fields

        private const string CmdPattern = @"^\{layout:(?<Cmd>[^\s\}]+)\}$";

        private const string ConvertPattern = @"^\{convert:(?<Converter>[^\s\}]+)\}$";

        private const string KeyPattern = @"^\{[^\s\}]+\}$";

        #endregion

        #region Methods

        public static void ExecuteKeyCommand(this KeyboardButton button, KeyboardKeyCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            Match layoutCmdMatch, layoutCmdMatch2, convertCmdMatch;
            if ((layoutCmdMatch = Regex.Match(command.CommandText, CmdPattern))?.Success == true)
            {
                //check the SecondaryCommandText to turn at the original keyboard language
                if (command.SecondaryCommandText != null &&
                    Localized.Instance.LastKeyboardCulture != null &&
                    (layoutCmdMatch2 = Regex.Match(command.SecondaryCommandText, CmdPattern))?.Success == true)
                {
                    var layoutCode2 = layoutCmdMatch2.Groups["Cmd"].Value + "." + Localized.Instance.LastKeyboardCulture.Name;
                    button.FindKeyboard()?.RequestLayoutChange(layoutCode2);
                }
                else
                {
                    var layoutCode = layoutCmdMatch.Groups["Cmd"].Value;
                    button.FindKeyboard()?.RequestLayoutChange(layoutCode);
                }
            }
            else if ((convertCmdMatch = Regex.Match(command.CommandText, ConvertPattern))?.Success == true)
            {
                var recourceKey = convertCmdMatch.Groups["Converter"].Value;
                if (button.TryFindResource(recourceKey) is System.Windows.Data.IValueConverter converter)
                {
                    var target = System.Windows.Input.Keyboard.FocusedElement;
                    if (target is TextBox text)
                    {
                        var culture = System.Globalization.CultureInfo.CurrentCulture;
                        text.Text = System.Convert.ToString(converter.Convert(text.Text, typeof(string), null, culture), culture);
                        text.SelectAll();
                    }
                    else if (target != null)
                    {
                        throw new NotSupportedException($"Cannot manage input element of type {target.GetType()} using keyboard conversion commands.");
                    }
                }
                else
                {
                    throw new ResourceReferenceKeyNotFoundException("Resource Key not found or not castable to IValueConverter.", recourceKey);
                }
            }
            else
            {
                command.SendKeys(out var key, out var text);
                if (key != System.Windows.Input.Key.None || !string.IsNullOrEmpty(text))
                {
                    button.FindKeyboard()?.FireKeyboardCommand(key, text);
                }
            }
        }

        public static void ExecuteKeyCommand(this KeyboardButton button)
            => button.ExecuteKeyCommand(button.KeyCommand);

        public static Ferretto.VW.App.Keyboards.Controls.Keyboard FindKeyboard(this KeyboardButton button)
            => button.FindAncestor<Keyboard>();

        public static void SendKeys(this KeyboardKeyCommand command)
            => command.SendKeys(out var _0, out var _1);

        private static T FindAncestor<T>(this DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
            {
                // cut the loop here
                return null;
            }
            return parent as T ?? FindAncestor<T>(parent);
        }

        private static void SendKeys(this KeyboardKeyCommand command, out System.Windows.Input.Key key, out string text)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            key = System.Windows.Input.Key.None;
            text = default;
            var txt = command.CommandText;
            if (Regex.IsMatch(txt, KeyPattern))
            {
                var cmd = txt.Substring(1, txt.Length - 2);
                if (Enum.TryParse<System.Windows.Input.Key>(cmd, out key))
                {
                    Ferretto.VW.App.Keyboards.SendKeys.Send(key);
                }
            }
            else if (!string.IsNullOrEmpty(txt))
            {
                text = txt;
                Ferretto.VW.App.Keyboards.SendKeys.Send(txt);
            }
        }

        #endregion
    }
}
