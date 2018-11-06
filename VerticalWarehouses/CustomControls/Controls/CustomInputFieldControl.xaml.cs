using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ferretto.VW.CustomControls.Controls
{
    public partial class CustomInputFieldControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty InputProperty = DependencyProperty.Register("InputText", typeof(string), typeof(CustomInputFieldControl), new PropertyMetadata(""));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomInputFieldControl), new PropertyMetadata(""));

        #endregion Fields

        #region Constructors

        public CustomInputFieldControl()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public string InputText
        {
            get => (string)this.GetValue(InputProperty);
            set { this.SetValue(InputProperty, value); this.RaisePropertyChanged("InputText"); }
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelProperty);
            set { this.SetValue(LabelProperty, value); this.RaisePropertyChanged("LabelText"); }
        }

        #endregion Properties

        #region Methods

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.InputTextBox.Text = this.InputTextBox.Text;
                var b = this.GetBindingExpression(InputProperty);
                b.UpdateSource();
            }
        }

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
