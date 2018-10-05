using System;
using System.Windows;

namespace Ferretto.Common.Controls
{
    public partial class TextBox : FormControl
    {
        #region Fields

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
            nameof(FieldName), typeof(string), typeof(TextBox), new PropertyMetadata(default(string), new PropertyChangedCallback(OnFieldNameChanged)));

        public static readonly DependencyProperty IsMultilineProperty = DependencyProperty.Register(
            nameof(IsMultiline), typeof(bool), typeof(TextBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

        #endregion Fields

        #region Constructors

        public TextBox()
        {
            this.InitializeComponent();
            this.GridTextBox.DataContext = this;

            this.DataContextChanged += this.TextBox_DataContextChanged;
            this.Loaded += this.TextBox_Loaded;
        }

        #endregion Constructors

        #region Properties

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set => this.SetValue(FieldNameProperty, value);
        }

        public bool IsMultiline
        {
            get => (bool)this.GetValue(IsMultilineProperty);
            set => this.SetValue(IsMultilineProperty, value);
        }

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        #endregion Properties

        #region Methods

        private static void OnFieldNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                SetTextBinding(textBox);
                textBox.Label = RetrieveLocalizedFieldName(textBox.InnerTextBox.DataContext, textBox.FieldName);
            }
        }

        private static void SetTextBinding(TextBox textBox)
        {
            textBox.InnerTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, textBox.FieldName);
        }

        private void TextBox_DataContextChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            this.InnerTextBox.DataContext = e.NewValue;

            if (this.FieldName != null)
            {
                SetTextBinding(this);
            }
        }

        private void TextBox_Loaded(Object sender, RoutedEventArgs e)
        {
            SetTextBinding(this);
            this.Label = RetrieveLocalizedFieldName(this.InnerTextBox.DataContext, this.FieldName);
        }

        #endregion Methods
    }
}
