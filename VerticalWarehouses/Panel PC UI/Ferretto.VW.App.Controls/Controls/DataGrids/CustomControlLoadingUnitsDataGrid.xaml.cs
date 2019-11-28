using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class CustomControlLoadingUnitsDataGrid : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(CustomControlLoadingUnitsDataGrid));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CustomControlLoadingUnitsDataGrid));

        #endregion

        #region Constructors

        public CustomControlLoadingUnitsDataGrid()
        {
            this.InitializeComponent();
            this.DataGrid.DataContext = this;
        }

        #endregion

        #region Properties

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => (object)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        #endregion
    }
}
