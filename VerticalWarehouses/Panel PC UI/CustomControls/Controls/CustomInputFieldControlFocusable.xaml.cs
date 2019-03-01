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
    /// Interaction logic for CustomInputFieldControlFocusableFocusable.xaml
    /// </summary>
    public partial class CustomInputFieldControlFocusable : UserControl, INotifyPropertyChanged
    {
        #region Fields

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(SolidColorBrush), typeof(CustomInputFieldControlFocusable));

        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.Register("Highlighted", typeof(bool), typeof(CustomInputFieldControlFocusable));

        public static readonly DependencyProperty InputProperty = DependencyProperty.Register("InputText", typeof(string), typeof(CustomInputFieldControlFocusable), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(CustomInputFieldControlFocusable), new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public CustomInputFieldControlFocusable()
        {
            this.InitializeComponent();
            var customInputFieldControlFocusable = this;
            this.LayoutRoot.DataContext = customInputFieldControlFocusable;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)this.GetValue(BorderColorProperty);
            set
            {
                this.SetValue(BorderColorProperty, value);
                this.RaisePropertyChanged(nameof(this.BorderColor));
            }
        }

        public bool Highlighted
        {
            get => (bool)this.GetValue(HighlightedProperty);
            set
            {
                this.SetValue(HighlightedProperty, value);
                this.RaisePropertyChanged(nameof(this.Highlighted));
            }
        }

        public string InputText
        {
            get => (string)this.GetValue(InputProperty);
            set
            {
                this.SetValue(InputProperty, value);
                this.RaisePropertyChanged(nameof(this.InputText));
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

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
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

        #endregion
    }
}
