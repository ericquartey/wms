using System.Windows.Controls;
using Ferretto.VW.App.Controls.Keyboards;

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
