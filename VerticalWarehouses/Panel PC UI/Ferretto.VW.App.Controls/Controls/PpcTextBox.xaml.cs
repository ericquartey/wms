using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Controls.Controls.Keyboards;

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

        #endregion

        #region Constructors

        public PpcTextBox()
        {
            this.InitializeComponent();
            var customInputFieldControlFocusable = this;
            this.LayoutRoot.DataContext = customInputFieldControlFocusable;
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

        private void OnKeyboardOpenHandler(object sender, InputEventArgs e)
        {
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

        #endregion
    }
}
