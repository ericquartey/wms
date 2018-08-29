using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
  /// <summary>
  /// Interaction logic for TextBox.xaml
  /// </summary>
  public partial class TextBox : UserControl
  {
    #region Dependency properties

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
      "Label", typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

    public string Label
    {
      get => (string) this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
      "Text", typeof(string), typeof(TextBox), new PropertyMetadata(default(string)));

    public string Text
    {
      get => (string) this.GetValue(TextProperty);
      set => this.SetValue(TextProperty, value);
    }

    #endregion

    #region Ctor

    public TextBox()
    {
      this.InitializeComponent();
    }

    #endregion
  }
}
