using System.Collections.Generic;
using System.Windows;
using DevExpress.Xpf.Navigation;

namespace Ferretto.Common.Controls
{
    public partial class FilterBar : TileBar
    {
        #region Fields

        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register(nameof(Filters), typeof(TileFilter), typeof(FilterBar));

        #endregion Fields

        #region Constructors

        public FilterBar()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.filterBar.SelectedIndex = 0;
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TileFilter> Filters
        {
            get => (IEnumerable<TileFilter>)this.GetValue(FiltersProperty);
            set => this.SetValue(FiltersProperty, value);
        }

        public static readonly DependencyProperty SelectedDataSourceProperty =
            DependencyProperty.Register(nameof(SelectedDataSource), typeof(object), typeof(FilterBar));

        public object SelectedDataSource
        {
            get => (object)this.GetValue(SelectedDataSourceProperty);
            set => this.SetValue(SelectedDataSourceProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnSelectedItemChanged(System.Object oldValue, System.Object newValue)
        {
            base.OnSelectedItemChanged(oldValue, newValue);
            this.SelectedDataSource = newValue;            
        }

        #endregion
    }
}
