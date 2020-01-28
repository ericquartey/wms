using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ferretto.VW.App.Keyboards
{
    public static class KeyboardKeyCommandExtensions
    {
        #region Fields

        private const string KeyPattern = @"^\{[^\s]+\}$";

        #endregion

        #region Methods

        public static void SendKeys(this KeyboardKeyCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            string text = command.Command;
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

        #endregion
    }
}
