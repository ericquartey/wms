using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.CustomControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomLabelTextBlockControl.xaml
    /// </summary>
    public partial class CustomProgressBarControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            "LabelText", typeof(string), typeof(CustomProgressBarControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProgressionValueProperty = DependencyProperty.Register(
            "ProgressionValue", typeof(int), typeof(CustomProgressBarControl));

        #endregion

        #region Constructors

        public CustomProgressBarControl()
        {
            this.InitializeComponent();
            var customProgressBar = this;
            this.LayoutRoot.DataContext = customProgressBar;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set
            {
                this.SetValue(LabelTextProperty, value);
                this.RaisePropertyChanged(nameof(this.LabelText));
            }
        }

        public int ProgressionValue
        {
            get => (int)this.GetValue(ProgressionValueProperty);
            set
            {
                this.SetValue(ProgressionValueProperty, value);
                this.RaisePropertyChanged(nameof(this.ProgressionValue));
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
