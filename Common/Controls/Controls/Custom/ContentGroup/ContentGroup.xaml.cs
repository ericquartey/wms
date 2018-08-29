using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ferretto.Common.Controls
{
  [ContentProperty("InnerContent")]
  public partial class ContentGroup : UserControl
  {
    #region Constructors

    public ContentGroup()
    {
      this.InitializeComponent();
    }

    #endregion

    #region InnerContent Dependency Property

    public static readonly DependencyProperty InnerContentProperty =
       DependencyProperty.Register("InnerContent", typeof(object), typeof(ContentGroup));

    public object InnerContent
    {
      get => this.GetValue(InnerContentProperty);
      set => this.SetValue(InnerContentProperty, value);
    }

    #endregion

    #region Label Dependency Property

    public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register("Label", typeof(string), typeof(ContentGroup));

    public string Label
    {
      get => (string)this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    #endregion
  }
}
