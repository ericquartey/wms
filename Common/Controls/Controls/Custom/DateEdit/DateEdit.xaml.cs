using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Editors;

namespace Ferretto.Common.Controls
{
  /// <summary>
  /// Interaction logic for DateEdit.xaml
  /// </summary>
  public partial class DateEdit : UserControl
  {
    #region Dependency properties

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
      "Label", typeof(string), typeof(DateEdit), new FrameworkPropertyMetadata(default(string)));

    public string Label
    {
      get => (string) this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty EditValueProperty = DependencyProperty.Register(
      "EditValue", typeof(DateTime?), typeof(DateEdit), new FrameworkPropertyMetadata(
        null,
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public DateTime? EditValue
    {
      get => (DateTime?) this.GetValue(EditValueProperty);
      set => this.SetValue(EditValueProperty, value);
    }

    public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
      "Mask", typeof(string), typeof(DateEdit), new FrameworkPropertyMetadata("d"));

    public string Mask
    {
      get => (string) this.GetValue(MaskProperty);
      set => this.SetValue(MaskProperty, value);
    }

    public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.Register(
      "MaskType", typeof(MaskType), typeof(DateEdit), new FrameworkPropertyMetadata(MaskType.DateTimeAdvancingCaret));

    public MaskType MaskType
    {
      get => (MaskType) this.GetValue(MaskTypeProperty);
      set => this.SetValue(MaskTypeProperty, value);
    }

    #endregion

    #region Ctor

    public DateEdit()
    {
      this.InitializeComponent();
    }

    #endregion
  }
}
