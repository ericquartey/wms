using System.Windows;
using System.Windows.Data;

namespace Ferretto.Common.Controls
{
    public partial class InfoText : FormControl
    {
        #region Fields

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
         nameof(FieldName), typeof(string), typeof(InfoText), new PropertyMetadata(default(string), new PropertyChangedCallback(OnFieldNameChanged)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(InfoText), new PropertyMetadata(default(string)));

        private const string FallbackValue = "-";

        #endregion Fields

        #region Constructors

        public InfoText()
        {
            this.InitializeComponent();
            this.InfoTextGrid.DataContext = this;

            this.DataContextChanged += this.InfoText_DataContextChanged;
            this.Loaded += this.InfoText_Loaded;
        }

        #endregion Constructors

        #region Properties

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set => this.SetValue(FieldNameProperty, value);
        }

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        private bool HasBindingForFieldName => this.InnerText.GetBindingExpression(ContentProperty)?.ResolvedSourcePropertyName == this.FieldName;

        #endregion Properties

        #region Methods

        private static void OnFieldNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoText infoText)
            {
                SetTextBinding(infoText);

                infoText.Label = RetrieveLocalizedFieldName(infoText.InnerText.DataContext, infoText.FieldName);
            }
        }

        private static void SetTextBinding(InfoText infoText)
        {
            if (infoText.FieldName == null || infoText.HasBindingForFieldName)
            {
                return;
            }

            var binding = new Binding(infoText.FieldName)
            {
                Mode = BindingMode.OneWay,
                FallbackValue = FallbackValue,
                TargetNullValue = FallbackValue
            };

            infoText.InnerText.SetBinding(ContentProperty, binding);
        }

        private void InfoText_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.InnerText.DataContext = e.NewValue;

            SetTextBinding(this);
        }

        private void InfoText_Loaded(object sender, RoutedEventArgs e)
        {
            SetTextBinding(this);

            this.Label = RetrieveLocalizedFieldName(this.InnerText.DataContext, this.FieldName);
        }

        #endregion Methods
    }
}
