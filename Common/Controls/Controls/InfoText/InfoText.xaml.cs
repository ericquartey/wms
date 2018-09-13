using System;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class InfoText : UserControl
    {
        #region Fields

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(InfoText), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(InfoText), new FrameworkPropertyMetadata(default(string))
            { CoerceValueCallback = CoerceValueWhenEmpty });

        #endregion Fields

        #region Constructors

        public InfoText()
        {
            this.InitializeComponent();
            this.InfoTextGrid.DataContext = this;
        }

        #endregion Constructors

        #region Properties

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

        #region Methods

        private static Object CoerceValueWhenEmpty(DependencyObject d, Object baseValue)
        {
            var textValue = (string)baseValue;

            return string.IsNullOrWhiteSpace(textValue) ? "-" : baseValue;
        }

        #endregion Methods
    }
}
