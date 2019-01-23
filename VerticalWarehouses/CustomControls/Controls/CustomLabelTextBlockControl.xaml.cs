using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomLabelTextBlockControl.xaml
    /// </summary>
    public partial class CustomLabelTextBlockControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(CustomLabelTextBlockControl), new PropertyMetadata(""));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomLabelTextBlockControl), new PropertyMetadata(""));

        #endregion Fields

        #region Constructors

        public CustomLabelTextBlockControl()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string ContentText
        {
            get => (string)this.GetValue(ContentTextProperty);
            set { this.SetValue(ContentTextProperty, value); this.RaisePropertyChanged("ContentText"); }
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
