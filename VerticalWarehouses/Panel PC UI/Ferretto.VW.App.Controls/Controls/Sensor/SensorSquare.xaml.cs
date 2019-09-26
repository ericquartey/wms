using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.CommonUtils.Enumerations;
using Prism.Commands;

namespace Ferretto.VW.App.Controls
{
    public partial class SensorSquare : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty BulletColorProperty = DependencyProperty.Register("BulletColor", typeof(SolidColorBrush), typeof(SensorSquare));

        public static readonly DependencyProperty FontSizeLabelProperty = DependencyProperty.Register("FontSizeLabel", typeof(double), typeof(SensorSquare), new PropertyMetadata(10D));

        public static readonly DependencyProperty HeightControlProperty = DependencyProperty.Register("HeightControl", typeof(double), typeof(SensorSquare), new FrameworkPropertyMetadata(22D, OnHeightControlChanged));

        public static readonly DependencyProperty IoMachineSensorProperty = DependencyProperty.Register("IoMachineSensor", typeof(IOMachineSensors), typeof(SensorSquare));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SensorSquare));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(SensorSquare), new PropertyMetadata("00"));

        public static readonly DependencyProperty SensorPortProperty = DependencyProperty.Register("SensorPort", typeof(IOMachineSensors), typeof(SensorSquare), new FrameworkPropertyMetadata(IOMachineSensors.NoValue, OnSensorPortChanged));

        public static readonly DependencyProperty SensorsProperty = DependencyProperty.Register("Sensors", typeof(bool[]), typeof(SensorSquare), new FrameworkPropertyMetadata(null, OnSensorsChanged));

        public static readonly DependencyProperty SensorStateProperty = DependencyProperty.Register("SensorState", typeof(bool), typeof(SensorSquare));

        private ICommand leftClickCommand;

        #endregion

        #region Constructors

        public SensorSquare()
        {
            this.InitializeComponent();

            var customSensorSquareControl = this;
            this.LayoutRoot.DataContext = customSensorSquareControl;
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

        public double FontSizeLabel
        {
            get => (double)this.GetValue(FontSizeLabelProperty);
            set
            {
                this.SetValue(FontSizeLabelProperty, value);
                this.RaisePropertyChanged(nameof(this.FontSizeLabel));
            }
        }

        public double HeightControl
        {
            get => (double)this.GetValue(HeightControlProperty);
            set
            {
                this.SetValue(HeightControlProperty, value);
                this.RaisePropertyChanged(nameof(this.HeightControl));
            }
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set
            {
                this.SetValue(IsReadOnlyProperty, value);
                this.RaisePropertyChanged(nameof(this.IsReadOnly));
            }
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set
            {
                this.SetValue(LabelTextProperty, value);
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

        public void UpdateHeightControl(double height)
        {
            this.Height = height;
            this.Width = height;
        }

        public void UpdateSensorState()
        {
            if (this.Sensors != null && this.SensorPort != IOMachineSensors.NoValue)
            {
                this.SensorState = this.Sensors[(int)this.SensorPort];
                return;
            }

            this.SensorState = false;
        }

        private static void OnHeightControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SensorSquare control && e.NewValue is double height)
            {
                control.UpdateHeightControl(height);
            }
        }

        private static void OnSensorPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SensorSquare control && e.NewValue is IOMachineSensors sensorStatus)
            {
                control.UpdateSensorState();
            }
        }

        private static void OnSensorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SensorSquare control && e.NewValue is bool[] sensors)
            {
                control.UpdateSensorState();
            }
        }

        private void ExecuteLeftClick()
        {
            if (!this.IsReadOnly)
            {
                this.SensorState = !this.SensorState;
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
