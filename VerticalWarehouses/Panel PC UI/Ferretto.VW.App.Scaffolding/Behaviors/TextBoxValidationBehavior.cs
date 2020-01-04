using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public abstract class TextBoxFilterBehavior : Behavior<TextBox>
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

        public abstract bool Validate(string text);

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            bool accept = true;
            if (this.IsEnabled)
            {
                TextBox box = sender as TextBox;
                int start = box.CaretIndex,
                    end = start;
                if (box.SelectionLength > 0)
                {
                    start = box.SelectionStart;
                    end = start + box.SelectionLength;
                }
                string actualText = box.Text;
                string previewText = string.Concat(actualText.Substring(0, start), e.Text, actualText.Substring(end));
                accept = this.Validate(previewText);
            }
            e.Handled = !accept;
        }

        public static readonly DependencyProperty IsEnabledProperty
                    = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TextBoxFilterBehavior), new PropertyMetadata(true));

        public bool IsEnabled
        {
            get => (bool)this.GetValue(IsEnabledProperty);
            set => this.SetValue(IsEnabledProperty, value);
        }

    }
}
