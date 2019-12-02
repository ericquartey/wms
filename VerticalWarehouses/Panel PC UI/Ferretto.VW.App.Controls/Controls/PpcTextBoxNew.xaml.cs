using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcTextBoxNew : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(
            nameof(BorderColor),
            typeof(SolidColorBrush),
            typeof(PpcTextBoxNew));

        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.Register(
            nameof(Highlighted),
            typeof(bool),
            typeof(PpcTextBoxNew));

        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(
            nameof(InputText),
            typeof(string),
            typeof(PpcTextBoxNew),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(PpcTextBoxNew),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Constructors

        public PpcTextBoxNew()
        {
            this.InitializeComponent();
            var customInputFieldControlFocusable = this;
            this.LayoutRoot.DataContext = customInputFieldControlFocusable;
        }

        #endregion

        #region Properties

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }

        public bool Highlighted
        {
            get => (bool)this.GetValue(HighlightedProperty);
            set => this.SetValue(HighlightedProperty, value);
        }

        public string InputText
        {
            get => (string)this.GetValue(InputTextProperty);
            set => this.SetValue(InputTextProperty, value);
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set => this.SetValue(LabelTextProperty, value);
        }

        #endregion

        #region Methods

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var b = this.GetBindingExpression(InputTextProperty);
                b?.UpdateSource();
            }
        }

        #endregion
    }
}
