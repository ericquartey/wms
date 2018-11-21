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
    public partial class CustomControlHighlightableBullet : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty BulletColorProperty = DependencyProperty.Register("BulletColor", typeof(SolidColorBrush), typeof(CustomControlHighlightableBullet));
        public static readonly DependencyProperty SensorStateProperty = DependencyProperty.Register("SensorState", typeof(bool), typeof(CustomControlHighlightableBullet));

        #endregion Fields

        #region Constructors

        public CustomControlHighlightableBullet()
        {
            this.InitializeComponent();
            this.root.DataContext = this;
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

        public bool SensorState
        {
            get => (bool)this.GetValue(SensorStateProperty);
            set { this.SetValue(SensorStateProperty, value); this.RaisePropertyChanged("SensorState"); }
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
