using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PpcBadge.xaml
    /// </summary>
    public partial class PpcBadge : UserControl
    {
        #region Fields

        public static readonly DependencyProperty TextBadgeProperty = DependencyProperty.Register(
            nameof(TextBadge),
            typeof(string),
            typeof(PpcBadge),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnLabelTextChanged)));

        #endregion

        #region Constructors

        public PpcBadge()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public string TextBadge
        {
            get => (string)this.GetValue(TextBadgeProperty);
            set => this.SetValue(TextBadgeProperty, value);
        }

        #endregion

        #region Methods

        private static void OnLabelTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcBadge control)
            {
                control.PpcBadge_TextBlock.Text = e.NewValue as string;
                if (e.NewValue == string.Format(App.Resources.VWApp.BayNumber, 0) ||
                    control.Visibility != Visibility.Visible)
                {
                    control.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion
    }
}
