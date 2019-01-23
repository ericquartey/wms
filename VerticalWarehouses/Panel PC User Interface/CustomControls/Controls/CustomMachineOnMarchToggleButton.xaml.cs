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
    /// Interaction logic for CustomMachineOnMarchToggleButton.xaml
    /// </summary>
    public partial class CustomMachineOnMarchToggleButton : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(CustomMachineOnMarchToggleButton));
        public static readonly DependencyProperty MachineOnMarchStateProperty = DependencyProperty.Register("MachineOnMarchState", typeof(bool), typeof(CustomMachineOnMarchToggleButton));
        public static readonly DependencyProperty MachineOnMarchStringProperty = DependencyProperty.Register("MachineOnMarchString", typeof(string), typeof(CustomMachineOnMarchToggleButton));
        public static readonly DependencyProperty RectangleBrushProperty = DependencyProperty.Register("RectangleBrush", typeof(SolidColorBrush), typeof(CustomMachineOnMarchToggleButton));

        #endregion Fields

        #region Constructors

        public CustomMachineOnMarchToggleButton()
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

        public bool MachineOnMarchState
        {
            get => (bool)this.GetValue(MachineOnMarchStateProperty);
            set { this.SetValue(MachineOnMarchStateProperty, value); this.RaisePropertyChanged("MachineOnMarchState"); }
        }

        public string MachineOnMarchString
        {
            get => (string)this.GetValue(MachineOnMarchStringProperty);
            set { this.SetValue(MachineOnMarchStringProperty, value); this.RaisePropertyChanged("MachineOnMarchString"); }
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
