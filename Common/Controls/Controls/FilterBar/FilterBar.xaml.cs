using System.Windows;
using DevExpress.Xpf.Navigation;

namespace Ferretto.Common.Controls
{
    public partial class FilterBar : TileBar
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
            get => (string) this.GetValue(FiltersProperty);
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
