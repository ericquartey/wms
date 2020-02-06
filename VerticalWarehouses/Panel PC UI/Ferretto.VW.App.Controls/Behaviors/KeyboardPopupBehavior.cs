using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Ferretto.VW.App.Controls.Behaviors
{
    public class KeyboardPopupBehavior : Behavior<TextBox>
    {
        #region Fields

        public static readonly DependencyProperty IsDoubleClickTriggerEnabledProperty
                    = DependencyProperty.RegisterAttached(nameof(IsDoubleClickTriggerEnabled), typeof(bool), typeof(KeyboardPopupBehavior), new PropertyMetadata(true));

        public static readonly DependencyProperty KeyboardLayoutCodeProperty =
                    DependencyProperty.Register(nameof(KeyboardLayoutCode), typeof(string), typeof(KeyboardPopupBehavior), new PropertyMetadata("lowercase"));

        #endregion

        #region Properties

        public bool IsDoubleClickTriggerEnabled
        {
            get => (bool)this.GetValue(IsDoubleClickTriggerEnabledProperty);
            set => this.SetValue(IsDoubleClickTriggerEnabledProperty, value);
        }

        public string KeyboardLayoutCode
        {
            get => (string)this.GetValue(KeyboardLayoutCodeProperty);
            set => this.SetValue(KeyboardLayoutCodeProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.TouchDown += this.TextBox_TouchDown;
            this.AssociatedObject.MouseDoubleClick += this.TextBox_MouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.TouchDown -= this.TextBox_TouchDown;
            base.OnDetaching();
        }

        private void OnOpenKeyboard(RoutedEventArgs e)
        {
            if (this.AssociatedObject.IsEnabled && !this.AssociatedObject.IsReadOnly)
            {
                e.Handled = true;
                this.OpenKeyboard();
            }
        }

        private void OpenKeyboard()
        {
            // show keyboard
            var dialog = new Keyboards.PpcKeyboards(this.AssociatedObject);
            dialog.DataContext = new Keyboards.PpcKeyboardsViewModel(this.KeyboardLayoutCode)
            {
                InputText = this.AssociatedObject.Text
            };
            dialog.Topmost = false;
            dialog.ShowInTaskbar = false;
            PpcDialogView.ShowDialog(dialog);
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.IsDoubleClickTriggerEnabled)
            {
                this.OnOpenKeyboard(e);
            }
        }

        private void TextBox_TouchDown(object sender, TouchEventArgs e)
        {
            this.OnOpenKeyboard(e);
        }

        #endregion
    }
}
