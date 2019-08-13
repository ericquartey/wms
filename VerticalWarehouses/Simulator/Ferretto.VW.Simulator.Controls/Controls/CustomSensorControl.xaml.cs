using System.ComponentModel;
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

        public static readonly DependencyProperty FontSizeLabelProperty = DependencyProperty.Register("FontSizeLabel", typeof(double), typeof(CustomSensorControl), new PropertyMetadata(10D));

        public static readonly DependencyProperty HeightControlProperty = DependencyProperty.Register("HeightControl", typeof(double), typeof(CustomSensorControl), new FrameworkPropertyMetadata(24D, OnHeightControlChanged));

        public static readonly DependencyProperty IoMachineSensorProperty = DependencyProperty.Register("IoMachineSensor", typeof(IOMachineSensors), typeof(CustomSensorControl));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomSensorControl), new PropertyMetadata("00"));

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
            this.Grid.Height = height;
            this.Grid.Width = height;
            this.MajorEllipse.Height = height;
            this.MajorEllipse.Width = height;
            if (height > 2D)
            {
                this.MinorEllipse.Height = height - 2D;
                this.MinorEllipse.Width = height - 2D;
            }
            else
            {
                this.MinorEllipse.Height = 2D;
                this.MinorEllipse.Width = 2D;
            }
        }

        public void UpdateMarginLabel(double margin)
        {
            this.TextBlock.Margin = new Thickness(margin, 0, 0, 0);
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
            if (d is CustomSensorControl control && e.NewValue is double height)
            {
                control.UpdateHeightControl(height);
            }
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
