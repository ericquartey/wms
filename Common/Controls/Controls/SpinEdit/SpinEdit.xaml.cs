using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for SpinEdit.xaml
    /// </summary>
    public partial class SpinEdit : UserControl
    {
        #region Fields

        public static readonly DependencyProperty EditValueProperty = DependencyProperty.Register(
            nameof(EditValue), typeof(int?), typeof(SpinEdit), new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
                    nameof(Label), typeof(string), typeof(SpinEdit), new FrameworkPropertyMetadata(default(string)));

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
            nameof(Mask), typeof(string), typeof(SpinEdit), new FrameworkPropertyMetadata("d"));

        public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.Register(
            nameof(MaskType), typeof(MaskType), typeof(SpinEdit),
            new FrameworkPropertyMetadata(MaskType.Numeric));

        #endregion Fields

        #region Constructors

        public SpinEdit()
        {
            this.InitializeComponent();
            this.GridSpinEdit.DataContext = this;
        }

        #endregion Constructors

        #region Properties

        public int? EditValue
        {
            get => (int?)this.GetValue(EditValueProperty);
            set => this.SetValue(EditValueProperty, value);
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

        #endregion Properties
    }
}
