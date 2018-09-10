using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
  public partial class FilterBar : UserControl
  {
    #region Constructors

    public FilterBar()
    {
      this.InitializeComponent();
    }

    #endregion

    #region Filters Dependency Property

    public static readonly DependencyProperty FiltersProperty =
    DependencyProperty.Register(nameof(Filters), typeof(string), typeof(FilterBar));

    public string Filters
    {
      get => (string)this.GetValue(FiltersProperty);
      set => this.SetValue(FiltersProperty, value);
    }

    #endregion
  }
}
