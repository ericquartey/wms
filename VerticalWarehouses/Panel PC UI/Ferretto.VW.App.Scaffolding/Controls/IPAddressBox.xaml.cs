using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.App.Scaffolding.Controls
{
    /// <summary>
    /// Interaction logic for IPAddressBox.xaml
    /// </summary>
    public partial class IPAddressBox : UserControl
    {
        public IPAddressBox()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(IPAddressBox), new PropertyMetadata(false));

        public static readonly DependencyProperty IPAddressProperty
            = DependencyProperty.Register("IPAddress", typeof(System.Net.IPAddress), typeof(IPAddressBox));

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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            bool accept = byte.TryParse(previewText, out byte _);
            e.Handled = !accept;
        }
    }
}
