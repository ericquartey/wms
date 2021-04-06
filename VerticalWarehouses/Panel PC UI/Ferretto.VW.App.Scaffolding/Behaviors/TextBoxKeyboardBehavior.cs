using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls.Keyboards;

namespace Ferretto.VW.App.Scaffolding.Behaviors
{
    public class TextBoxKeyboardBehavior : TogglableBehavior<TextBox>
    {
        #region Fields

        public static readonly DependencyProperty IsDoubleClickTriggerEnabledProperty
                    = DependencyProperty.RegisterAttached(nameof(IsDoubleClickTriggerEnabled), typeof(bool), typeof(TextBoxKeyboardBehavior), new PropertyMetadata(true));

        public static readonly DependencyProperty KeyboardCaptionProperty
                    = DependencyProperty.RegisterAttached(nameof(KeyboardCaption), typeof(string), typeof(TextBoxKeyboardBehavior));

        public static readonly DependencyProperty KeyboardProperty
                    = DependencyProperty.RegisterAttached("Keyboard", typeof(KeyboardType), typeof(TextBoxKeyboardBehavior), new PropertyMetadata(KeyboardType.QWERTY));

        #endregion

        #region Properties

        public bool IsDoubleClickTriggerEnabled
        {
            get => (bool)this.GetValue(IsDoubleClickTriggerEnabledProperty);
            set => this.SetValue(IsDoubleClickTriggerEnabledProperty, value);
        }

        public KeyboardType Keyboard
        {
            get => (KeyboardType)this.GetValue(KeyboardProperty);
            set => this.SetValue(KeyboardProperty, value);
        }

        public string KeyboardCaption
        {
            get => (string)this.GetValue(KeyboardCaptionProperty);
            set => this.SetValue(KeyboardCaptionProperty, value);
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
            this.AssociatedObject.MouseDoubleClick -= this.TextBox_MouseDoubleClick;
            base.OnDetaching();
        }

        private void OnOpenKeyboard(RoutedEventArgs e)
        {
            if (this.IsEnabled && this.AssociatedObject.IsEnabled && !this.AssociatedObject.IsReadOnly)
            {
                e.Handled = true;
                this.OpenKeyboard();
            }
        }

        private void OpenKeyboard()
        {
            var box = this.AssociatedObject;

            switch (this.Keyboard)
            {
                case KeyboardType.QWERTY:
                    var ppcKeyboard = new PpcKeyboard();
                    var vmKeyboard = new PpcKeypadsPopupViewModel();
                    ppcKeyboard.DataContext = vmKeyboard;
                    vmKeyboard.Update(this.KeyboardCaption, box.Text ?? string.Empty);
                    ppcKeyboard.Topmost = false;
                    ppcKeyboard.ShowInTaskbar = false;
                    PpcMessagePopup.ShowDialog(ppcKeyboard);
                    box.Text = vmKeyboard.ScreenText;
                    break;

                //case KeyboardType.Multi:
                //    var keyboard = new Keyboards.Keyboards();
                //    var vmMulti = new PpcKeypadsPopupViewModel();
                //    keyboard.Keyboardsss = vmMulti.Keyboards;
                //    vmMulti.Update(this.KeyboardCaption, box.Text ?? string.Empty);
                //    keyboard.Topmost = false;
                //    keyboard.ShowInTaskbar = false;
                //    PpcMessagePopup.ShowDialog(keyboard);
                //    box.Text = vmMulti.ScreenText;
                //    break;

                case KeyboardType.NumpadCenter:
                    var ppcMessagePopup = new PpcNumpadCenterPopup();
                    var vm = new PpcKeypadsPopupViewModel();
                    ppcMessagePopup.DataContext = vm;
                    vm.Update(this.KeyboardCaption, box.Text ?? string.Empty);
                    ppcMessagePopup.Topmost = false;
                    ppcMessagePopup.ShowInTaskbar = false;
                    PpcMessagePopup.ShowDialog(ppcMessagePopup);
                    box.Text = vm.ScreenText;
                    break;

                case KeyboardType.Numpad:
                    var ppcNumpadPopup = new PpcNumpadPopup();
                    var vmNumpad = new PpcKeypadsPopupViewModel();
                    ppcNumpadPopup.DataContext = vmNumpad;
                    vmNumpad.Update(this.KeyboardCaption, box.Text ?? string.Empty);
                    ppcNumpadPopup.Topmost = false;
                    ppcNumpadPopup.ShowInTaskbar = false;
                    PpcMessagePopup.ShowAnchorDialog(ppcNumpadPopup);
                    box.Text = vmNumpad.ScreenText;
                    break;

                default:
                    break;
            }
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
