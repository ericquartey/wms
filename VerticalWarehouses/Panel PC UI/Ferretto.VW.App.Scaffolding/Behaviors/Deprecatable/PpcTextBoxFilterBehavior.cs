using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public abstract class PpcTextBoxFilterBehavior : PpcTextBoxBehavior
    {
        protected override void OnDetaching()
        {
            this.TextBox.PreviewTextInput -= this.TextBox_PreviewTextInput;
            base.OnDetaching();
        }

        protected override void OnTextBoxAttached()
        {
            base.OnTextBoxAttached();
            this.TextBox.PreviewTextInput += this.TextBox_PreviewTextInput;
        }

        public abstract bool Validate(string text);

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var accept = true;
            if (this.IsEnabled)
            {
                var box = sender as TextBox;
                int start = box.CaretIndex,
                    end = start;
                if (box.SelectionLength > 0)
                {
                    start = box.SelectionStart;
                    end = start + box.SelectionLength;
                }
                var actualText = box.Text;
                var previewText = string.Concat(actualText.Substring(0, start), e.Text, actualText.Substring(end));
                accept = this.Validate(previewText);
            }
            e.Handled = !accept;
        }
    }
}
