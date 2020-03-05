using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Behaviors
{
    public class TextBoxAutoSelectBehavior : TogglableBehavior<TextBox>
    {
        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.GotFocus += this.TextBox_GotFocus;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.GotFocus -= this.TextBox_GotFocus;
            base.OnDetaching();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.IsEnabled)
            {
                this.AssociatedObject.SelectAll();
            }
        }

        #endregion
    }
}
