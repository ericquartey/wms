using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Controls.Keyboards;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.App.Keyboards;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Key = System.Windows.Input.Key;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PccDateBox.xaml
    /// </summary>
    public partial class PccDateBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(
            nameof(BorderColor),
            typeof(SolidColorBrush),
            typeof(PccDateBox));

        public static readonly DependencyProperty DateFormatProperty = DependencyProperty.Register(
            nameof(DateFormat),
            typeof(string),
            typeof(PccDateBox),
            new PropertyMetadata("dd/MM/yyyy"));

        public static readonly DependencyProperty InputDateProperty = DependencyProperty.Register(
            nameof(InputDate),
            typeof(DateTimeOffset?),
            typeof(PccDateBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(PccDateBox));

        public static readonly DependencyProperty KeyboardProperty = DependencyProperty.Register(
            nameof(Keyboard),
            typeof(KeyboardType),
            typeof(PccDateBox),
            new PropertyMetadata(KeyboardType.QWERTY));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(PccDateBox),
            new PropertyMetadata(string.Empty));

        private IMachineIdentityWebService machineIdentityWebService;

        private bool touchHelperEnabled;

        #endregion

        #region Constructors

        public PccDateBox()
        {
            this.InitializeComponent();

            var customInputFieldControlFocusable = this;
            this.Root.DataContext = customInputFieldControlFocusable;

            this.Loaded += (s, e) => { this.OnAppearedAsync(); };
            this.Unloaded += (s, e) => { this.Disappear(); };
            this.IsEnabledChanged += this.PpcTextBox_IsEnabledChanged;
        }

        #endregion

        #region Properties

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }

        public string DateFormat
        {
            get => (string)this.GetValue(DateFormatProperty);
            set => this.SetValue(DateFormatProperty, value);
        }

        public DateTimeOffset? InputDate
        {
            get => (DateTimeOffset?)this.GetValue(InputDateProperty);
            set => this.SetValue(InputDateProperty, value);
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
            this.touchHelperEnabled = await this.machineIdentityWebService.GetTouchHelperEnableAsync();
            this.KeyboardButton.Visibility = this.touchHelperEnabled && this.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
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

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var b = this.GetBindingExpression(InputDateProperty);
                b?.UpdateSource();
            }
        }

        private void OpenKeyboard()
        {
            if (this.IsEnabled && !this.IsReadOnly && this.InputDateBox.IsEnabled && !this.InputDateBox.IsReadOnly)
            {
                var tempTextBox = new TextBox() { Text = this.InputDateBox.EditValue is null ? "" : this.InputDateBox.DateTime.ToString(this.DateFormat) };

                tempTextBox.PopupKeyboard(KeyboardLayoutCodes.Numeric, this.LabelText, TimeSpan.FromSeconds(60));

                if (DateTime.TryParse(tempTextBox.Text, out DateTime res) || res == null)
                {
                    this.InputDateBox.DateTime = res;
                }
            }
        }

        private void PpcTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.touchHelperEnabled)
            {
                this.KeyboardButton.Visibility = this.touchHelperEnabled && this.IsEnabled && !this.IsReadOnly ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion
    }
}
