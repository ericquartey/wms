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
using Ferretto.VW.App.Resources;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for PpcBadge.xaml
    /// </summary>
    public partial class PpcBayBadge : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BayNumberProperty =
            DependencyProperty.Register(nameof(BayNumber), typeof(int), typeof(PpcBayBadge),
                new PropertyMetadata(0, new PropertyChangedCallback(BayNumberChanged)));

        #endregion

        #region Constructors

        public PpcBayBadge()
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
            get
            {
                int val = (int)this.GetValue(BayNumberProperty);
                this.PpcBadge_TextBlock.Text = Localized.Get("General.BayNumber") + " " + val.ToString();
                return val;
            }
            set => this.SetValue(BayNumberProperty, value);
        }

        #endregion

        #region Methods

        private static void BayNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcBayBadge control)
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

        #endregion
    }
}
