using System.Windows;
using System.Windows.Controls;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    /// <summary>
    /// Interaction logic for NotificationItem.xaml
    /// </summary>
    public partial class NotificationItem : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
           nameof(Mode),
           typeof(StatusType),
           typeof(NotificationItem),
           new PropertyMetadata(null));

        #endregion

        #region Constructors

        public NotificationItem()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public StatusType Mode { get => (StatusType)this.GetValue(ModeProperty); set => this.SetValue(ModeProperty, value); }

        #endregion

        #region Methods

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            this.Visibility = Visibility.Hidden;
            parentWindow.Close();
        }

        #endregion
    }
}
