using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Controls.Keyboards;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Key = System.Windows.Input.Key;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcTextBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(
            nameof(BorderColor),
            typeof(SolidColorBrush),
            typeof(PpcTextBox));

        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.Register(
            nameof(Highlighted),
            typeof(bool),
            typeof(PpcTextBox));

        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(
            nameof(InputText),
            typeof(string),
            typeof(PpcTextBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(PpcTextBox));

        public static readonly DependencyProperty KeyboardProperty = DependencyProperty.Register(
            nameof(Keyboard),
            typeof(KeyboardType),
            typeof(PpcTextBox),
            new PropertyMetadata(KeyboardType.QWERTY));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(PpcTextBox),
            new PropertyMetadata(string.Empty));

        private IMachineIdentityWebService machineIdentityWebService;

        #endregion

        #region Constructors

        public PpcTextBox()
        {
            this.InitializeComponent();
            var customInputFieldControlFocusable = this;
            this.LayoutRoot.DataContext = customInputFieldControlFocusable;

            this.Loaded += (s, e) => { this.OnAppearedAsync(); };
            this.Unloaded += (s, e) => { this.Disappear(); };
        }

        #endregion

        #region Properties

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }

        public bool Highlighted
        {
            get => (bool)this.GetValue(HighlightedProperty);
            set => this.SetValue(HighlightedProperty, value);
        }

        public string InputText
        {
            get => (string)this.GetValue(InputTextProperty);
            set => this.SetValue(InputTextProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        public KeyboardType Keyboard
        {
            get => (KeyboardType)this.GetValue(KeyboardProperty);
            set => this.SetValue(KeyboardProperty, value);
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set => this.SetValue(LabelTextProperty, value);
        }

        #endregion

        #region Methods

        protected void Disappear()
        {
            this.machineIdentityWebService = null;
        }

        protected async void OnAppearedAsync()
        {
            this.machineIdentityWebService = ServiceLocator.Current.GetInstance<IMachineIdentityWebService>();
            var enable = await this.machineIdentityWebService.GetTouchHelperEnableAsync();
            this.KeyboardButton.Visibility = enable && this.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void KeyboardButton_TouchUp(object sender, TouchEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OnKeyboardOpenHandlerOld(object sender, InputEventArgs e)
        {
            if (this.IsReadOnly)
            {
                return;
            }

            switch (this.Keyboard)
            {
                case KeyboardType.QWERTY:
                    var ppcKeyboard = new PpcKeyboard();
                    var vmKeyboard = new PpcKeypadsPopupViewModel();
                    ppcKeyboard.DataContext = vmKeyboard;
                    vmKeyboard.Update(this.LabelText, this.InputText?.ToString() ?? string.Empty);
                    ppcKeyboard.Topmost = false;
                    ppcKeyboard.ShowInTaskbar = false;
                    PpcMessagePopup.ShowDialog(ppcKeyboard);
                    this.InputText = vmKeyboard.ScreenText;
                    break;

                case KeyboardType.NumpadCenter:
                    var ppcMessagePopup = new PpcNumpadCenterPopup();
                    var vm = new PpcKeypadsPopupViewModel();
                    ppcMessagePopup.DataContext = vm;
                    vm.Update(this.LabelText, this.InputText?.ToString() ?? string.Empty);
                    ppcMessagePopup.Topmost = false;
                    ppcMessagePopup.ShowInTaskbar = false;
                    PpcMessagePopup.ShowDialog(ppcMessagePopup);
                    this.InputText = vm.ScreenText;
                    break;

                case KeyboardType.Numpad:
                    var ppcNumpadPopup = new PpcNumpadPopup();
                    var vmNumpad = new PpcKeypadsPopupViewModel();
                    ppcNumpadPopup.DataContext = vmNumpad;
                    vmNumpad.Update(this.LabelText, this.InputText?.ToString() ?? string.Empty);
                    ppcNumpadPopup.Topmost = false;
                    ppcNumpadPopup.ShowInTaskbar = false;
                    PpcMessagePopup.ShowAnchorDialog(ppcNumpadPopup);
                    this.InputText = vmNumpad.ScreenText;
                    break;

                default:
                    break;
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var b = this.GetBindingExpression(InputTextProperty);
                b?.UpdateSource();
            }
        }

        private void OpenKeyboard()
        {
            if (this.IsEnabled && !this.IsReadOnly && this.InputTextBox.IsEnabled && !this.InputTextBox.IsReadOnly)
            {
                this.InputTextBox.PopupKeyboard(caption: this.LabelText, timeout: TimeSpan.FromSeconds(60));
            }
        }

        #endregion
    }
}
