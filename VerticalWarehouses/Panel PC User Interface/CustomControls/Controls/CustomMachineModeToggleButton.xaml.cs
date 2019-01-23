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
    /// Interaction logic for CustomMachineModeToggleButton.xaml
    /// </summary>
    public partial class CustomMachineModeToggleButton : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(CustomMachineModeToggleButton));
        public static readonly DependencyProperty MachineModeStateProperty = DependencyProperty.Register("MachineModeState", typeof(bool), typeof(CustomMachineModeToggleButton));
        public static readonly DependencyProperty MachineModeStringProperty = DependencyProperty.Register("MachineModeString", typeof(string), typeof(CustomMachineModeToggleButton));
        public static readonly DependencyProperty RectangleBrushProperty = DependencyProperty.Register("RectangleBrush", typeof(SolidColorBrush), typeof(CustomMachineModeToggleButton));

        #endregion Fields

        #region Constructors

        public CustomMachineModeToggleButton()
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

        public bool MachineModeState
        {
            get => (bool)this.GetValue(MachineModeStateProperty);
            set { this.SetValue(MachineModeStateProperty, value); this.RaisePropertyChanged("MachineModeState"); }
        }

        public string MachineModeString
        {
            get => (string)this.GetValue(MachineModeStringProperty);
            set { this.SetValue(MachineModeStringProperty, value); this.RaisePropertyChanged("MachineModeString"); }
        }

        public SolidColorBrush RectangleBrush
        {
            get => (SolidColorBrush)this.GetValue(RectangleBrushProperty);
            set { this.SetValue(RectangleBrushProperty, value); this.RaisePropertyChanged("RectangleBrush"); }
        }

        #endregion Properties

        #region Methods

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}
