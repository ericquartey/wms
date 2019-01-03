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
using Prism.Commands;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomControlLoggedUser.xaml
    /// </summary>
    public partial class CustomControlLoggedUser : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty IsPopupOpenProperty = DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(CustomControlLoggedUser));
        public static readonly DependencyProperty LogOffCommandProperty = DependencyProperty.Register("LogOffCommand", typeof(ICommand), typeof(CustomControlLoggedUser));
        public static readonly DependencyProperty OpenClosePopupCommandProperty = DependencyProperty.Register("OpenClosePopupCommand", typeof(ICommand), typeof(CustomControlLoggedUser));
        public static readonly DependencyProperty UserTextProperty = DependencyProperty.Register("UserText", typeof(string), typeof(CustomControlLoggedUser));

        #endregion Fields

        #region Constructors

        public CustomControlLoggedUser()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public bool IsPopupOpen
        {
            get => (bool)this.GetValue(IsPopupOpenProperty);
            set { this.SetValue(IsPopupOpenProperty, value); this.RaisePropertyChanged("IsPopupOpen"); }
        }

        public ICommand LogOffCommand
        {
            get => (ICommand)this.GetValue(LogOffCommandProperty);
            set { this.SetValue(LogOffCommandProperty, value); this.RaisePropertyChanged("LogOffCommand"); }
        }

        public ICommand OpenClosePopupCommand
        {
            get => (ICommand)this.GetValue(OpenClosePopupCommandProperty);
            set { this.SetValue(OpenClosePopupCommandProperty, value); this.RaisePropertyChanged("OpenClosePopupCommand"); }
        }

        public string UserText
        {
            get => (string)this.GetValue(UserTextProperty);
            set { this.SetValue(UserTextProperty, value); this.RaisePropertyChanged("UserText"); }
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
