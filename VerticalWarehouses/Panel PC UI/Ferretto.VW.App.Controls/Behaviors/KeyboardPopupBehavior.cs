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

        public static readonly DependencyProperty InactiveTimeoutProperty =
                            DependencyProperty.Register(nameof(InactiveTimeout), typeof(TimeSpan), typeof(KeyboardPopupBehavior));

        public static readonly DependencyProperty IsDoubleClickTriggerEnabledProperty
                            = DependencyProperty.RegisterAttached(nameof(IsDoubleClickTriggerEnabled), typeof(bool), typeof(KeyboardPopupBehavior), new PropertyMetadata(true));

        public static readonly DependencyProperty KeyboardLabelProperty =
                    DependencyProperty.Register(nameof(KeyboardLabel), typeof(string), typeof(KeyboardPopupBehavior));

        public static readonly DependencyProperty KeyboardLayoutCodeProperty =
                            DependencyProperty.Register(nameof(KeyboardLayoutCode), typeof(string), typeof(KeyboardPopupBehavior), new PropertyMetadata("lowercase"));

        #endregion

        #region Properties

        public TimeSpan InactiveTimeout
        {
            get => (TimeSpan)this.GetValue(InactiveTimeoutProperty);
            set => this.SetValue(InactiveTimeoutProperty, value);
        }

        public bool IsDoubleClickTriggerEnabled
        {
            get => (bool)this.GetValue(IsDoubleClickTriggerEnabledProperty);
            set => this.SetValue(IsDoubleClickTriggerEnabledProperty, value);
        }

        public string KeyboardLabel
        {
            get => (string)this.GetValue(KeyboardLabelProperty);
            set => this.SetValue(KeyboardLabelProperty, value);
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
            this.AssociatedObject.TouchUp += this.TextBox_TouchUp;
            this.AssociatedObject.MouseDoubleClick += this.TextBox_MouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.TouchUp -= this.TextBox_TouchUp;
            base.OnDetaching();
        }

        private void OpenKeyboard()
        {
            if (!this.AssociatedObject.IsEnabled || this.AssociatedObject.IsReadOnly)
            {
                return;
            }
            // show keyboard
            var dialog = new Keyboards.PpcKeyboards(this.AssociatedObject);
            dialog.DataContext = new Keyboards.PpcKeyboardsViewModel(this.KeyboardLayoutCode)
            {
                InputText = this.AssociatedObject.Text,
                LabelText = this.KeyboardLabel,
                InactiveTimeout = this.InactiveTimeout
            };
            dialog.Topmost = false;
            dialog.ShowInTaskbar = false;
            PpcDialogView.ShowDialog(dialog);
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.IsDoubleClickTriggerEnabled)
            {
                this.OpenKeyboard();
            }
        }

        private void TextBox_TouchUp(object sender, TouchEventArgs e)
        {
            // e.Handled = true;
            this.Dispatcher.BeginInvoke(new Action(this.OpenKeyboard));
        }

        #endregion
    }
}
