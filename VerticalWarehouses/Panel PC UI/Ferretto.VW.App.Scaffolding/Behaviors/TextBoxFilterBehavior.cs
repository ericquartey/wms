using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public abstract class TextBoxFilterBehavior : TextBoxValidationBehavior
    {
        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewTextInput -= this.TextBox_PreviewTextInput;
            base.OnDetaching();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewTextInput += this.TextBox_PreviewTextInput;
        }

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
