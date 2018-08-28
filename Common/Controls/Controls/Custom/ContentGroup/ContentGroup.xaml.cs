using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ferretto.Common.Controls
{
  [ContentProperty("InnerContent")]
  public partial class ContentGroup : UserControl
  {
    public ContentGroup()
    {
      this.InitializeComponent();

      this.LayoutRoot.DataContext = this;
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
