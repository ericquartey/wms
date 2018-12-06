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
    /// <summary>
    /// Interaction logic for CustomInputText.xaml
    /// </summary>
    public partial class CustomInputText : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register("InputText", typeof(string), typeof(CustomInputText));
        public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(CustomInputText));
        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomInputText));
        public static readonly DependencyProperty TextBoxBorderBrushProperty = DependencyProperty.Register("TextBoxBorderBrush", typeof(SolidColorBrush), typeof(CustomInputText));

        #endregion Fields

        #region Constructors

        public CustomInputText()
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
            get => (string)this.GetValue(InputTextProperty);
            set { this.SetValue(InputTextProperty, value); this.RaisePropertyChanged("InputText"); }
        }

        public bool IsHighlighted
        {
            get => (bool)this.GetValue(IsHighlightedProperty);
            set { this.SetValue(IsHighlightedProperty, value); this.RaisePropertyChanged("IsHighlighted"); }
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set { this.SetValue(LabelTextProperty, value); this.RaisePropertyChanged("LabelText"); }
        }

        public SolidColorBrush TextBoxBorderBrush
        {
            get => (SolidColorBrush)this.GetValue(TextBoxBorderBrushProperty);
            set { this.SetValue(TextBoxBorderBrushProperty, value); this.RaisePropertyChanged("TextBoxBorderBrush"); }
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
