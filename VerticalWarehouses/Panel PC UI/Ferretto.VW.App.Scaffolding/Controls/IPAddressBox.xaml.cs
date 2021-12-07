using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Keyboards;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Scaffolding.Controls
{
    /// <summary>
    /// Interaction logic for IPAddressBox.xaml
    /// </summary>
    public partial class IPAddressBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IPAddressProperty
            = DependencyProperty.Register("IPAddress", typeof(System.Net.IPAddress), typeof(IPAddressBox));

        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(IPAddressBox), new PropertyMetadata(false));

        private TextBox lastSelectedTextBox;

        private IMachineIdentityWebService machineIdentityWebService;

        private bool touchHelperEnabled;

        #endregion

        #region Constructors

        public IPAddressBox()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) => { this.OnAppearedAsync(); };
            this.Unloaded += (s, e) => { this.Disappear(); };
            this.IsEnabledChanged += this.IPAddressBox_IsEnabledChanged; ;
        }

        #endregion

        #region Properties

        public System.Net.IPAddress IPAddress
        {
            get => (System.Net.IPAddress)this.GetValue(IPAddressProperty);
            set => this.SetValue(IPAddressProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        #endregion

        #region Methods

        protected void Disappear()
        {
            this.machineIdentityWebService = null;
            this.lastSelectedTextBox = null;
        }

        protected async void OnAppearedAsync()
        {
            this.machineIdentityWebService = ServiceLocator.Current.GetInstance<IMachineIdentityWebService>();
            this.touchHelperEnabled = await this.machineIdentityWebService.GetTouchHelperEnableAsync();
            this.KeyboardButton.Visibility = this.touchHelperEnabled && this.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            this.lastSelectedTextBox = sender as TextBox;
            this.KeyboardButton.IsEnabled = true;
        }

        private void IPAddressBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.touchHelperEnabled)
            {
                this.KeyboardButton.Visibility = this.touchHelperEnabled && this.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void KeyboardButton_TouchUp(object sender, TouchEventArgs e)
        {
            this.OpenKeyboard();
        }

        private void OpenKeyboard()
        {
            if (this.lastSelectedTextBox != null)
            {
                if (this.IsEnabled && !this.IsReadOnly && this.lastSelectedTextBox.IsEnabled && !this.lastSelectedTextBox.IsReadOnly)
                {
                    this.lastSelectedTextBox.PopupKeyboard(layoutCode: "Numpad", timeout: TimeSpan.FromSeconds(60));

                    this.lastSelectedTextBox = null;
                    this.KeyboardButton.IsEnabled = false;
                }
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            var accept = byte.TryParse(previewText, out var _);
            e.Handled = !accept;
        }

        #endregion
    }
}
