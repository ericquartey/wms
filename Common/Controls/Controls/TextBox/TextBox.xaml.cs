using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class TextBox : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
           nameof(FieldName), typeof(string), typeof(TextBox), new PropertyMetadata(default(string), new PropertyChangedCallback(OnFieldNameChanged)));

        public static readonly DependencyProperty IsMultilineProperty = DependencyProperty.Register(
                   nameof(IsMultiline), typeof(bool), typeof(TextBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
                    nameof(Label), typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

        private static readonly string DisplayAttributeNameProperty = "Name";
        private static readonly string DisplayAttributeResourceTypeProperty = "ResourceType";

        #endregion Fields

        #region Constructors

        public TextBox()
        {
            this.InitializeComponent();
            this.InnerLabel.DataContext = this;

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
                textBox.InnerTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, textBox.FieldName);

                textBox.RetrieveLabel();
            }
        }

        private void RetrieveLabel()
        {
            if (this.InnerTextBox.DataContext == null || this.FieldName == null)
            {
                return;
            }

            var displayAttributeData = this.InnerTextBox.DataContext.GetType()
                .GetProperty(this.FieldName)
                .CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType == typeof(DisplayAttribute));

            if (displayAttributeData == null)
            {
                return;
            }

            var nameArgumentValue = (string)displayAttributeData.NamedArguments.Single(arg => arg.MemberName == DisplayAttributeNameProperty).TypedValue.Value;

            var resourceTypeArgument = displayAttributeData.NamedArguments.Single(arg => arg.MemberName == DisplayAttributeResourceTypeProperty);

            var resourceClassType = resourceTypeArgument.TypedValue.Value as Type;

            var propertyInfo = resourceClassType.GetProperty(nameArgumentValue);

            this.Label = (string)propertyInfo.GetValue(null);
        }

        private void TextBox_DataContextChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            this.InnerTextBox.DataContext = e.NewValue;

            if (this.FieldName != null)
            {
                this.InnerTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, this.FieldName);
            }
        }

        private void TextBox_Loaded(Object sender, RoutedEventArgs e)
        {
            this.InnerTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, this.FieldName);
            this.RetrieveLabel();
        }

        #endregion Methods
    }
}
