using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Keyboards;
using Microsoft.Xaml.Behaviors;

namespace Ferretto.VW.App.Controls.Behaviors
{
    public class KeyboardPopupBehavior : KeyboardPopupBehaviorBase<TextBox>
    {
        #region Methods

        protected override bool IsKeyboardEnabled(Control ctrl)
        {
            if (ctrl is TextBox txtBox)
            {
                return txtBox.IsEnabled && !txtBox.IsReadOnly;
            }
            return base.IsKeyboardEnabled(ctrl);
        }

        protected override void OpenKeyboard()
        {
            this.AssociatedObject.PopupKeyboard(this.KeyboardLayoutCode, this.KeyboardLabel, this.InactiveTimeout);
        }

        #endregion
    }
}
