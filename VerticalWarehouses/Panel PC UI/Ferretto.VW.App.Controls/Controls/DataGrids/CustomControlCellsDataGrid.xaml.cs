using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CustomControlCellStatisticsDataGrid.xaml
    /// </summary>
    public partial class CustomControlCellsDataGrid : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(CustomControlCellsDataGrid));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CustomControlCellsDataGrid));

        #endregion

        #region Constructors

        public CustomControlCellsDataGrid()
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
