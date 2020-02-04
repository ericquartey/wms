using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Ferretto.VW.App.Controls.Behaviors
{
    public class KeyboardPopupBehavior : Behavior<TextBox>
    {
        #region Fields

        private bool _isManipulating;

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.TouchDown += this.AssociatedObject_TouchDown;
            this.AssociatedObject.TouchUp += this.AssociatedObject_TouchUp;
            this.AssociatedObject.GotKeyboardFocus += this.AssociatedObject_GotKeyboardFocus;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.TouchDown -= this.AssociatedObject_TouchDown;
            this.AssociatedObject.TouchUp -= this.AssociatedObject_TouchUp;
            this.AssociatedObject.GotKeyboardFocus -= this.AssociatedObject_GotKeyboardFocus;
            base.OnDetaching();
        }

        private void AssociatedObject_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (this._isManipulating)
            {
                // showkeyboard
            }
        }

        private void AssociatedObject_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this._isManipulating = false;
        }

        private void AssociatedObject_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this._isManipulating = false;
        }

        #endregion
    }
}
