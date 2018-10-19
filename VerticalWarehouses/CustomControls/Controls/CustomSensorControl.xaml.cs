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
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomSensorControl), new PropertyMetadata(""));

        #endregion Fields

        #region Constructors

        public CustomSensorControl()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public SolidColorBrush BulletColor
        {
            get => (SolidColorBrush)this.GetValue(BulletColorProperty);
            set { this.SetValue(BulletColorProperty, value); this.RaisePropertyChanged("BulletColor"); }
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
