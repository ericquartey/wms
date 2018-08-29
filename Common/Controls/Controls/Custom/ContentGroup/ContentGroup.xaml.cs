using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ferretto.Common.Controls
{
  [ContentProperty("ControlContent")]
  public partial class ContentGroup : UserControl
  {
    #region Constructors

    public ContentGroup()
    {
      this.InitializeComponent();
    }

    #endregion

    #region InnerContent Dependency Property

    public static readonly DependencyProperty ControlContentProperty =
       DependencyProperty.Register(nameof(ControlContent), typeof(object), typeof(ContentGroup));

    public object ControlContent
    {
      get => this.GetValue(ControlContentProperty);
      set => this.SetValue(ControlContentProperty, value);
    }

    #endregion

    #region Label Dependency Property

    public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register(nameof(Label), typeof(string), typeof(ContentGroup));

    public string Label
    {
      get => (string)this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    #endregion
  }
}
