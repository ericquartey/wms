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
    /// Interaction logic for CustomControlMainWindowUserComboBoxItem.xaml
    /// </summary>
    public partial class CustomControlMainWindowUserComboBoxItem : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(CustomControlMainWindowUserComboBoxItem));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomControlMainWindowUserComboBoxItem), new PropertyMetadata(""));

        #endregion Fields

        #region Constructors

        public CustomControlMainWindowUserComboBoxItem()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public ICommand CustomCommand
        {
            get => (ICommand)this.GetValue(CustomCommandProperty);
            set { this.SetValue(CustomCommandProperty, value); this.RaisePropertyChanged("CustomCommand"); }
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelProperty);
            set { this.SetValue(LabelProperty, value); this.RaisePropertyChanged("LabelText"); }
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
