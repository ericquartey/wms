using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static readonly DependencyProperty BayNumberProperty =
            DependencyProperty.Register(nameof(BayNumber), typeof(int), typeof(PpcBadge),
                new PropertyMetadata(0, new PropertyChangedCallback(BayNumberChanged)));

        public static readonly DependencyProperty TextBadgeProperty =
                    DependencyProperty.Register(nameof(TextBadge), typeof(string), typeof(PpcBadge),
                new PropertyMetadata(string.Empty, new PropertyChangedCallback(TextBadgeChanged)));

        #endregion

        #region Constructors

        public PpcBadge()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => (int)this.GetValue(BayNumberProperty);
            set => this.SetValue(BayNumberProperty, value);
        }

        public string TextBadge
        {
            get => (string)this.GetValue(TextBadgeProperty);
            set => this.SetValue(TextBadgeProperty, value);
        }

        #endregion

        #region Methods

        private static void BayNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcBadge control)
            {
                if (control.Visibility != Visibility.Visible ||
                    control.BayNumber.Equals(0))
                {
                    control.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.Visibility = Visibility.Visible;
                }
            }
        }

        private static void TextBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcBadge control)
            {
                control.PpcBadge_TextBlock.Text = e.NewValue as string;
            }
        }

        #endregion
    }
}
