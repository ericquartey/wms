using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
  public partial class InfoText : UserControl
  {
    #region Dependency properties

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
      nameof(Label), typeof(string), typeof(InfoText), new PropertyMetadata(default(string)));

    public string Label
    {
      get => (string) this.GetValue(LabelProperty);
      set => this.SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
      nameof(Text), typeof(string), typeof(InfoText), new PropertyMetadata(default(string)));

    public string Text
    {
      get => (string) this.GetValue(TextProperty);
      set => this.SetValue(TextProperty, value);
    }

    #endregion

    #region Ctor

    public InfoText()
    {
      this.InitializeComponent();
      this.InfoTextGrid.DataContext = this;
    }

    #endregion
  }
}
