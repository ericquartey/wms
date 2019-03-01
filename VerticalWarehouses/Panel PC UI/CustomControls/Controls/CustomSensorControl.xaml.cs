using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.VW.CustomControls.Controls
{
    public partial class CustomSensorControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty BulletColorProperty = DependencyProperty.Register("BulletColor", typeof(SolidColorBrush), typeof(CustomSensorControl));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomSensorControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SensorStateProperty = DependencyProperty.Register("SensorState", typeof(bool), typeof(CustomSensorControl));

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
