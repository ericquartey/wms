using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ferretto.Common.Controls
{
  [ContentProperty("InnerContent")]
  public partial class ContentGroup : UserControl
  {
    public ContentGroup()
    {
      this.InitializeComponent();
    }

    public static readonly DependencyProperty InnerContentProperty =
       DependencyProperty.Register("InnerContent", typeof(object), typeof(ContentGroup));

    public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register("Label", typeof(string), typeof(ContentGroup));

    public object InnerContent
    {
      get { return this.GetValue(InnerContentProperty); }
      set { this.SetValue(InnerContentProperty, value); }
    }

    public string Label
    {
      get { return (string)this.GetValue(LabelProperty); }
      set { this.SetValue(LabelProperty, value); }
    }

  }
}
