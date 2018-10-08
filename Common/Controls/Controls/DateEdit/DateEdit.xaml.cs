using System;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
{
    public partial class DateEdit : FormControl
    {
        #region Fields

        public static readonly DependencyProperty EditValueProperty = DependencyProperty.Register(
            nameof(EditValue), typeof(DateTime?), typeof(DateEdit), new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
            nameof(FieldName), typeof(string), typeof(DateEdit), new PropertyMetadata(default(string), new PropertyChangedCallback(OnFieldNameChanged)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(DateEdit), new FrameworkPropertyMetadata(default(string)));

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
            nameof(Mask), typeof(string), typeof(DateEdit), new FrameworkPropertyMetadata("d"));

        public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.Register(
            nameof(MaskType), typeof(MaskType), typeof(DateEdit),
            new FrameworkPropertyMetadata(MaskType.DateTimeAdvancingCaret));

        #endregion Fields

        #region Constructors

        public DateEdit()
        {
            this.InitializeComponent();
            this.GridDateEdit.DataContext = this;

            this.DataContextChanged += this.DateEdit_DataContextChanged;
            this.Loaded += this.DateEdit_Loaded;
        }

        #endregion Constructors

        #region Properties

        public DateTime? EditValue
        {
            get => (DateTime?)this.GetValue(EditValueProperty);
            set => this.SetValue(EditValueProperty, value);
        }

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

        public string Mask
        {
            get => (string)this.GetValue(MaskProperty);
            set => this.SetValue(MaskProperty, value);
        }

        public MaskType MaskType
        {
            get => (MaskType)this.GetValue(MaskTypeProperty);
            set => this.SetValue(MaskTypeProperty, value);
        }

        private bool HasBindingForFieldName => this.InnerDateEdit.GetBindingExpression(BaseEdit.EditValueProperty)?.ResolvedSourcePropertyName == this.FieldName;

        #endregion Properties

        #region Methods

        private static void OnFieldNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateEdit dateEdit)
            {
                SetTextBinding(dateEdit);

                dateEdit.Label = RetrieveLocalizedFieldName(dateEdit.DataContext, dateEdit.FieldName);
            }
        }

        private static void SetTextBinding(DateEdit dateEdit)
        {
            if (dateEdit.FieldName == null || dateEdit.HasBindingForFieldName)
            {
                return;
            }

            var binding = new Binding($"{nameof(DataContext)}.{dateEdit.FieldName}")
            {
                Mode = BindingMode.TwoWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(ComboBox)
                }
            };

            dateEdit.InnerDateEdit.SetBinding(BaseEdit.EditValueProperty, binding);
        }

        private void DateEdit_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.DataContext = e.NewValue;

            SetTextBinding(this);
        }

        private void DateEdit_Loaded(object sender, RoutedEventArgs e)
        {
            SetTextBinding(this);

            this.Label = RetrieveLocalizedFieldName(this.DataContext, this.FieldName);
        }

        #endregion Methods
    }
}
