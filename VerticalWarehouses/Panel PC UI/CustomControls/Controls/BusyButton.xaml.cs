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
    /// Interaction logic for BusyButton.xaml
    /// </summary>
    public partial class BusyButton : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(BusyButton));

        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register("ButtonContent", typeof(string), typeof(BusyButton), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsWorkingProperty = DependencyProperty.Register("IsWorking", typeof(bool), typeof(BusyButton), new PropertyMetadata(false));

        #endregion

        #region Constructors

        public BusyButton()
        {
            this.InitializeComponent();
            this.layoutRoot.DataContext = this;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public ICommand ButtonCommand
        {
            get => (ICommand)this.GetValue(ButtonCommandProperty);
            set
            {
                this.SetValue(ButtonCommandProperty, value);
                this.RaisePropertyChanged(nameof(this.ButtonCommand));
            }
        }

        public string ButtonContent
        {
            get => (string)this.GetValue(ButtonContentProperty);
            set
            {
                this.SetValue(ButtonContentProperty, value);
                this.RaisePropertyChanged(nameof(this.ButtonContent));
            }
        }

        public bool IsWorking
        {
            get => (bool)this.GetValue(IsWorkingProperty);
            set
            {
                this.SetValue(IsWorkingProperty, value);
                this.RaisePropertyChanged(nameof(this.IsWorking));
            }
        }

        #endregion

        #region Methods

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
