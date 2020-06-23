using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.Installer.Controls
{
    public partial class StatusBar : UserControl
    {
        public static readonly DependencyProperty MessageProperty
            = DependencyProperty.Register(nameof(Message), typeof(string), typeof(StatusBar));

        public StatusBar()
        {
            this.InitializeComponent();
        }

        public string Message
        {
            get => (string)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }
    }
}
