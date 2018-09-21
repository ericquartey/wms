using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class TextBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IsMultilineProperty = DependencyProperty.Register(
           nameof(IsMultiline), typeof(bool), typeof(TextBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
                    nameof(Label), typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

        #endregion Fields

        #region Constructors

        public TextBox()
        {
            this.InitializeComponent();
            this.GridTextBox.DataContext = this;
        }

        #endregion Constructors

        #region Properties

        public bool IsMultiline
        {
            get => (bool)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        #endregion Properties
    }
}
