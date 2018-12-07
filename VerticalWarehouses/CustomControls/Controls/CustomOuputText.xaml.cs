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

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomOuputText.xaml
    /// </summary>
    public partial class CustomOuputText : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomOuputText));
        public static readonly DependencyProperty OutputTextProperty = DependencyProperty.Register("OutputText", typeof(string), typeof(CustomOuputText));

        #endregion Fields

        #region Constructors

        public CustomOuputText()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set { this.SetValue(LabelTextProperty, value); this.RaisePropertyChanged("LabelText"); }
        }

        public string OutputText
        {
            get => (string)this.GetValue(OutputTextProperty);
            set { this.SetValue(OutputTextProperty, value); this.RaisePropertyChanged("OutputText"); }
        }

        #endregion Properties

        #region Methods

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Methods
    }
}
