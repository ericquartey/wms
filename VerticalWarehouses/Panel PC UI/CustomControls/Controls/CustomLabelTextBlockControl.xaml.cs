using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcLabeledText : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register(
            nameof(ContentText),
            typeof(string),
            typeof(PpcLabeledText),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(PpcLabeledText),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public PpcLabeledText()
        {
            this.InitializeComponent();
            var customLabelTextBlockControl = this;
            this.LayoutRoot.DataContext = customLabelTextBlockControl;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public string ContentText
        {
            get => (string)this.GetValue(ContentTextProperty);
            set
            {
                this.SetValue(ContentTextProperty, value);
                this.RaisePropertyChanged(nameof(this.ContentText));
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
