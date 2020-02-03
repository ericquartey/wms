using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

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
                command.SendKeys();
            }
        }

        public static void ExecuteKeyCommand(this KeyboardButton button)
            => button.ExecuteKeyCommand(button.KeyCommand);

        public static Ferretto.VW.App.Keyboards.Keyboard FindKeyboard(this KeyboardButton button)
            => button.FindAncestor<Keyboard>();

        public static void SendKeys(this KeyboardKeyCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            string text = command.CommandText;
            if (Regex.IsMatch(text, KeyPattern))
            {
                string cmd = text.Substring(1, text.Length - 2);
                if (Enum.TryParse<System.Windows.Input.Key>(cmd, out var key))
                {
                    Ferretto.VW.App.Keyboards.SendKeys.Send(key);
                }
            }
            else if (!string.IsNullOrEmpty(text))
            {
                Ferretto.VW.App.Keyboards.SendKeys.Send(text);
            }
        }

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

        #endregion
    }
}
