using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ferretto.VW.App.Keyboards;

namespace Ferretto.VW.App.Controls.Keyboards
{
    public static class KeyboardPopupExtensions
    {
        #region Methods

        public static void PopupKeyboard(this TextBox ctrl, string layoutCode = KeyboardLayoutCodes.Lowercase, string caption = default, TimeSpan timeout = default)
            => PopupKeyboard(ctrl, TextBox.TextProperty, typeof(string), layoutCode, caption, timeout);

        public static void PopupKeyboard(this Control ctrl, DependencyProperty dependencyProperty, Type outputType, string layoutCode = KeyboardLayoutCodes.Lowercase,
            string caption = default, TimeSpan timeout = default)
        {
            // show keyboard
            var dialog = new Keyboards.PpcKeyboards(ctrl, dependencyProperty, outputType);
            object value = ctrl.GetValue(dependencyProperty);
            dialog.DataContext = new Keyboards.PpcKeyboardsViewModel(layoutCode)
            {
                InputText = Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture),
                LabelText = caption,
                InactiveTimeout = timeout,
            };
            dialog.Topmost = false;
            dialog.ShowInTaskbar = false;
            PpcDialogView.ShowDialog(dialog);
        }

        #endregion
    }
}
