using System;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
{
    public partial class SpinEdit : FormControl
    {
        #region Fields

        public static readonly DependencyProperty EditValueTypeProperty = DependencyProperty.Register(
            nameof(EditValueType), typeof(Type), typeof(SpinEdit), new FrameworkPropertyMetadata(typeof(int)));

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
            nameof(FieldName), typeof(string), typeof(SpinEdit), new PropertyMetadata(default(string), new PropertyChangedCallback(OnFieldNameChanged)));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(SpinEdit), new FrameworkPropertyMetadata(default(string)));

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
            nameof(Mask), typeof(string), typeof(SpinEdit), new FrameworkPropertyMetadata("d"));

        public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.Register(
            nameof(MaskType), typeof(MaskType), typeof(SpinEdit), new FrameworkPropertyMetadata(MaskType.Numeric));

        #endregion Fields

        #region Constructors

        public SpinEdit()
        {
            this.InitializeComponent();
            this.GridSpinEdit.DataContext = this;

            this.DataContextChanged += this.SpinEdit_DataContextChanged;
            this.Loaded += this.SpinEdit_Loaded;
        }

        #endregion Constructors

        #region Properties

        public Type EditValueType
        {
            get => (Type)this.GetValue(EditValueTypeProperty);
            set => this.SetValue(EditValueTypeProperty, value);
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

        private bool HasBindingForFieldName => this.InnerSpinEdit.GetBindingExpression(BaseEdit.EditValueProperty)?.ResolvedSourcePropertyName == this.FieldName;

        #endregion Properties

        #region Methods

        private static void OnFieldNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpinEdit spinEdit)
            {
                SetTextBinding(spinEdit);
                spinEdit.Label = RetrieveLocalizedFieldName(spinEdit.DataContext, spinEdit.FieldName);
            }
        }

        private static void SetTextBinding(SpinEdit spinEdit)
        {
            if (spinEdit.FieldName == null || spinEdit.HasBindingForFieldName)
            {
                return;
            }

            var binding = new Binding($"{nameof(DataContext)}.{spinEdit.FieldName}")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(SpinEdit)
                }
            };

            spinEdit.InnerSpinEdit.SetBinding(BaseEdit.EditValueProperty, binding);
        }

        private void SpinEdit_DataContextChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            SetTextBinding(this);
        }

        private void SpinEdit_Loaded(Object sender, RoutedEventArgs e)
        {
            SetTextBinding(this);

            this.Label = RetrieveLocalizedFieldName(this.DataContext, this.FieldName);
        }

        #endregion Methods
    }
}
