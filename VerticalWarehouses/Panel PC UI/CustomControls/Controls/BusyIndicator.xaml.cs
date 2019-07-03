using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for BusyIndicator.xaml
    /// </summary>
    public partial class BusyIndicator : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty IsWorkingProperty = DependencyProperty.Register("IsWorking", typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false));

        #endregion

        #region Constructors

        public BusyIndicator()
        {
            this.InitializeComponent();
            this.rootLayout.DataContext = this;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public bool IsWorking
        {
            get => (bool)this.GetValue(IsWorkingProperty);
            set
            {
                this.SetValue(IsWorkingProperty, value);
                this.RaisePropertyChanged(nameof(this.IsWorking));
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
