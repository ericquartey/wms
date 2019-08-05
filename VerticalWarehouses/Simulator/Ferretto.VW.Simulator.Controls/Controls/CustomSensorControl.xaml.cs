using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.CommonUtils.Enumerations;
using Prism.Commands;

namespace Ferretto.VW.Simulator.Controls.Controls
{
    public partial class CustomSensorControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty BulletColorProperty = DependencyProperty.Register("BulletColor", typeof(SolidColorBrush), typeof(CustomSensorControl));

        public static readonly DependencyProperty IoMachineSensorProperty = DependencyProperty.Register("IoMachineSensor", typeof(IOMachineSensors), typeof(CustomSensorControl));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomSensorControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SensorPortProperty = DependencyProperty.Register("SensorPort", typeof(IOMachineSensors), typeof(CustomSensorControl), new FrameworkPropertyMetadata(IOMachineSensors.NoValue, OnSensorPortChanged));

        public static readonly DependencyProperty SensorsProperty = DependencyProperty.Register("Sensors", typeof(bool[]), typeof(CustomSensorControl), new FrameworkPropertyMetadata(null, OnSensorsChanged));

        public static readonly DependencyProperty SensorStateProperty = DependencyProperty.Register("SensorState", typeof(bool), typeof(CustomSensorControl));

        private ICommand leftClickCommand;

        #endregion

        #region Constructors

        public CustomSensorControl()
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

        public ICommand LeftClickCommand => this.leftClickCommand ?? (this.leftClickCommand = new DelegateCommand(() => this.ExecuteLeftClick()));

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
            if (d is CustomSensorControl control && e.NewValue is IOMachineSensors sensorStatus)
            {
                control.UpdateSensorState();
            }
        }

        private static void OnSensorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomSensorControl control && e.NewValue is bool[] sensors)
            {
                control.UpdateSensorState();
            }
        }

        private void ExecuteLeftClick()
        {
            this.SensorState = !this.SensorState;
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
