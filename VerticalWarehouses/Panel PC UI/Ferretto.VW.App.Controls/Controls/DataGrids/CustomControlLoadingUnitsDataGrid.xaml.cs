using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        private string firstSort;

        private ListSortDirection lastDirection;

        #endregion

        #region Constructors

        public CustomControlLoadingUnitsDataGrid()
        {
            this.InitializeComponent();
            this.DataGrid.DataContext = this;
            this.firstSort = "";
        }

        #endregion

        #region Properties

        #region Method

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

        private void AddSortColumn(DataGrid sender, string sortColumn, ListSortDirection direction)
        {
            var cView = CollectionViewSource.GetDefaultView(sender.ItemsSource);
            cView.SortDescriptions.Add(new SortDescription(sortColumn, direction));
            //Add the sort arrow on the DataGridColumn
            foreach (var col in sender.Columns.Where(x => x.SortMemberPath == sortColumn))
            {
                col.SortDirection = direction;
            }
        }

        private void dataGridName_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var dgSender = (DataGrid)sender;
            var cView = CollectionViewSource.GetDefaultView(dgSender.ItemsSource);

            //Alternate between ascending/descending if the same column is clicked
            ListSortDirection direction = ListSortDirection.Ascending;
            if (cView.SortDescriptions.FirstOrDefault().PropertyName == e.Column.SortMemberPath)
            {
                direction = cView.SortDescriptions.FirstOrDefault().Direction == ListSortDirection.Descending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            }

            //To this point the default sort functionality is implemented

            //Now check the wanted columns and add multiple sort
            if (cView.SortDescriptions.Count >= 2 || cView.SortDescriptions.Count == 0)
            {
                this.lastDirection = direction;
                cView.SortDescriptions.Clear();
                this.AddSortColumn((DataGrid)sender, e.Column.SortMemberPath, direction);
            }
            else if (e.Column.SortMemberPath != this.firstSort)
            {
                this.firstSort = e.Column.SortMemberPath;
                this.lastDirection = direction;
                this.AddSortColumn((DataGrid)sender, this.firstSort, direction);
            }
            else if (e.Column.SortMemberPath == this.firstSort && this.lastDirection != direction)
            {
                this.lastDirection = direction;
                cView.SortDescriptions.Clear();
                this.AddSortColumn((DataGrid)sender, e.Column.SortMemberPath, direction);
            }
            e.Handled = true;
        }

        #endregion
    }
}
