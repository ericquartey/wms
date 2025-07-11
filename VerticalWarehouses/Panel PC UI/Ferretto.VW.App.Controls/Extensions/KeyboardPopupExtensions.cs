﻿using System;
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
            string caption = default, TimeSpan timeout = default, bool isPassword = false)
        {
            // show keyboard
            var dialog = new PpcKeyboards(ctrl, dependencyProperty, outputType);
            var value = ctrl.GetValue(dependencyProperty);
            dialog.DataContext = new PpcKeyboardsViewModel(layoutCode)
            {
                InputText = Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture),
                LabelText = caption,
                InactiveTimeout = timeout,
                IsPassword = isPassword
            };
            dialog.Topmost = false;
            dialog.ShowInTaskbar = false;
            PpcDialogView.ShowDialog(dialog);
        }

        #endregion
    }
}
