using System.Windows;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public class PpcTextBoxAutoSelectBehavior : PpcTextBoxBehavior
    {
        protected override void OnTextBoxAttached()
        {
            base.OnTextBoxAttached();
            this.TextBox.GotFocus += this.TextBox_GotFocus;
        }

        protected override void OnDetaching()
        {
            this.TextBox.GotFocus -= this.TextBox_GotFocus;
            base.OnDetaching();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.IsEnabled)
            {
                this.TextBox.SelectAll();
            }
        }
    }
}
