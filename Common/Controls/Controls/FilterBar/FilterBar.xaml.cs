using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class FilterBar : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register(nameof(Filters), typeof(TileFilter), typeof(FilterBar));

        #endregion Fields

        #region Constructors

        public FilterBar()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TileFilter> Filters
        {
            get => (IEnumerable<TileFilter>)this.GetValue(FiltersProperty);
            set => this.SetValue(FiltersProperty, value);
        }

        #endregion Properties
    }
}
