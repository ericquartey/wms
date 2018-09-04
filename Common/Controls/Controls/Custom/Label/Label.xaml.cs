using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
  public partial class Label : UserControl
  {
    #region Dependency properties

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
      nameof(Content), typeof(string), typeof(Label), new PropertyMetadata(default(string)));

    public string Content
    {
      get => (string) this.GetValue(ContentProperty);
      set => this.SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
      nameof(Text), typeof(string), typeof(Label), new PropertyMetadata(default(string)));

    public string Text
    {
      get => (string) this.GetValue(TextProperty);
      set => this.SetValue(TextProperty, value);
    }

    #endregion

    #region Ctor

    public Label()
    {      
      this.InitializeComponent();
      this.LabelGrid.DataContext = this;
    }

    #endregion
  }
}
