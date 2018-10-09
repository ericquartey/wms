using System.Collections;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
{
    public partial class ComboBox : FormControl
    {
        #region Fields

        public static readonly DependencyProperty AllowNullInputProperty = DependencyProperty.Register(
            nameof(AllowNullInput), typeof(bool), typeof(ComboBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty AutoCompleteProperty = DependencyProperty.Register(
            nameof(AutoComplete), typeof(bool), typeof(ComboBox), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register(
            nameof(DisplayMember), typeof(string), typeof(ComboBox), new PropertyMetadata("Description"));

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
            nameof(FieldName), typeof(string), typeof(ComboBox), new PropertyMetadata(default(string), new PropertyChangedCallback(OnFieldNameChanged)));

        public static readonly DependencyProperty IsTextEditableProperty = DependencyProperty.Register(
            nameof(IsTextEditable), typeof(bool), typeof(ComboBox), new PropertyMetadata(false));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable), typeof(ComboBox), new PropertyMetadata(default(IEnumerable)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(ComboBox), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(ComboBox), new UIPropertyMetadata());

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register(
            nameof(SelectedText), typeof(string), typeof(ComboBox), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ValueMemberProperty = DependencyProperty.Register(
            nameof(ValueMember), typeof(string), typeof(ComboBox), new PropertyMetadata("Value"));

        #endregion Fields

        #region Constructors

        public ComboBox()
        {
            this.InitializeComponent();
            this.gridComboBox.DataContext = this;

            this.DataContextChanged += this.ComboBox_DataContextChanged;
            this.Loaded += this.ComboBox_Loaded;
        }

        #endregion Constructors

        #region Properties

        public bool AllowNullInput
        {
            get => (bool)this.GetValue(AllowNullInputProperty);
            set => this.SetValue(AllowNullInputProperty, value);
        }

        public bool AutoComplete
        {
            get => (bool)this.GetValue(AutoCompleteProperty);
            set => this.SetValue(AutoCompleteProperty, value);
        }

        public string DisplayMember
        {
            get => (string)this.GetValue(DisplayMemberProperty);
            set => this.SetValue(DisplayMemberProperty, value);
        }

        public string FieldName
        {
            get => (string)this.GetValue(FieldNameProperty);
            set => this.SetValue(FieldNameProperty, value);
        }

        public bool IsTextEditable
        {
            get => (bool)this.GetValue(IsTextEditableProperty);
            set => this.SetValue(IsTextEditableProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public string SelectedText
        {
            get => (string)this.GetValue(SelectedTextProperty);
            set => this.SetValue(SelectedTextProperty, value);
        }

        public string Text => this.dxeComboBoxEdit.SelectedText;

        public string ValueMember
        {
            get => (string)this.GetValue(ValueMemberProperty);
            set => this.SetValue(ValueMemberProperty, value);
        }

        private bool HasBindingForFieldName => this.dxeComboBoxEdit.GetBindingExpression(BaseEdit.EditValueProperty)?.ResolvedSourcePropertyName == this.FieldName;

        #endregion Properties

        #region Methods

        private static void OnFieldNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                SetTextBinding(comboBox);

                comboBox.Label = RetrieveLocalizedFieldName(comboBox.DataContext, comboBox.FieldName);
            }
        }

        private static void SetTextBinding(ComboBox comboBox)
        {
            if (comboBox.FieldName == null || comboBox.HasBindingForFieldName)
            {
                return;
            }

            var binding = new Binding($"{nameof(DataContext)}.{comboBox.FieldName}")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(ComboBox)
                }
            };

            comboBox.dxeComboBoxEdit.SetBinding(BaseEdit.EditValueProperty, binding);
        }

        private void ComboBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.DataContext = e.NewValue;

            SetTextBinding(this);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            SetTextBinding(this);

            this.Label = RetrieveLocalizedFieldName(this.DataContext, this.FieldName);
        }

        #endregion Methods
    }
}
