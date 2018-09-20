using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class FilterBar : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register(nameof(Filters), typeof(object), typeof(FilterBar));

        public static readonly DependencyProperty SelectedDataSourceProperty =
            DependencyProperty.Register(nameof(SelectedDataSource), typeof(object), typeof(FilterBar));

        #endregion Fields

        #region Constructors

        public FilterBar()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public object Filters
        {
            get => this.GetValue(FiltersProperty);
            set => this.SetValue(FiltersProperty, value);
        }

        public object SelectedDataSource
        {
            get => this.GetValue(SelectedDataSourceProperty);
            set => this.SetValue(SelectedDataSourceProperty, value);
        }

        #endregion Properties
    }
}
