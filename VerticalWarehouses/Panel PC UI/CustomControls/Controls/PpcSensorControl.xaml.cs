using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.VW.CommonUtils.Enumerations;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcSensorControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty BulletColorProperty = DependencyProperty.Register(
            nameof(BulletColor),
            typeof(SolidColorBrush),
            typeof(PpcSensorControl));

        public static readonly DependencyProperty IoMachineSensorProperty = DependencyProperty.Register(
            "IoMachineSensor",
            typeof(IOMachineSensors),
            typeof(PpcSensorControl));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(PpcSensorControl),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SensorPortProperty = DependencyProperty.Register(
            nameof(SensorPort),
            typeof(IOMachineSensors),
            typeof(PpcSensorControl),
            new FrameworkPropertyMetadata(IOMachineSensors.NoValue, OnSensorPortChanged));

        public static readonly DependencyProperty SensorsProperty = DependencyProperty.Register(
            nameof(Sensors),
            typeof(bool[]),
            typeof(PpcSensorControl),
            new FrameworkPropertyMetadata(null, OnSensorsChanged));

        public static readonly DependencyProperty SensorStateProperty = DependencyProperty.Register(
            nameof(SensorState),
            typeof(bool),
            typeof(PpcSensorControl));

        #endregion

        #region Constructors

        public PpcSensorControl()
        {
            this.InitializeComponent();
            var customSensorControl = this;
            this.LayoutRoot.DataContext = customSensorControl;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public SolidColorBrush BulletColor
        {
            get => (SolidColorBrush)this.GetValue(BulletColorProperty);
            set
            {
                this.SetValue(BulletColorProperty, value);
                this.RaisePropertyChanged(nameof(this.BulletColor));
            }
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelProperty);
            set
            {
                this.SetValue(LabelProperty, value);
                this.RaisePropertyChanged(nameof(this.LabelText));
            }
        }

        public IOMachineSensors SensorPort
        {
            get => (IOMachineSensors)this.GetValue(SensorPortProperty);
            set
            {
                this.SetValue(SensorPortProperty, value);
                this.RaisePropertyChanged(nameof(this.SensorPort));
            }
        }

        public bool[] Sensors
        {
            get => (bool[])this.GetValue(SensorsProperty);
            set
            {
                this.SetValue(SensorsProperty, value);
                this.RaisePropertyChanged(nameof(this.Sensors));
            }
        }

        public bool SensorState
        {
            get => (bool)this.GetValue(SensorStateProperty);
            set
            {
                this.SetValue(SensorStateProperty, value);
                this.RaisePropertyChanged(nameof(this.SensorState));
            }
        }

        #endregion

        #region Methods

        public void UpdateSensorState()
        {
            if (this.Sensors != null && this.SensorPort != IOMachineSensors.NoValue)
            {
                this.SensorState = this.Sensors[(int)this.SensorPort];
                return;
            }

            this.SensorState = false;
        }

        private static void OnSensorPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcSensorControl control && e.NewValue is IOMachineSensors)
            {
                control.UpdateSensorState();
            }
        }

        private static void OnSensorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcSensorControl control && e.NewValue is bool[])
            {
                control.UpdateSensorState();
            }
        }

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
