using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Ferretto.VW.App.Keyboards.Controls;

namespace Ferretto.VW.App.Keyboards
{
    public static class KeyboardKeyCommandExtensions
    {
        #region Fields

        private const string CmdPattern = @"^\{layout:(?<Cmd>[^\s\}]+)\}$";

        private const string KeyPattern = @"^\{[^\s\}]+\}$";

        #endregion

        #region Methods

        public static void ExecuteKeyCommand(this KeyboardButton button, KeyboardKeyCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            Match layoutCmdMatch = Regex.Match(command.CommandText, CmdPattern);
            if (layoutCmdMatch?.Success == true)
            {
                string layoutCode = layoutCmdMatch.Groups["Cmd"].Value;
                button.FindKeyboard()?.RequestLayoutChange(layoutCode);
            }
            else
            {
                command.SendKeys(out System.Windows.Input.Key key, out string text);
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
            => command.SendKeys(out System.Windows.Input.Key _0, out string _1);

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
            string txt = command.CommandText;
            if (Regex.IsMatch(txt, KeyPattern))
            {
                string cmd = txt.Substring(1, txt.Length - 2);
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
