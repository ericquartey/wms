using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Modules.Operator.Models;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class WaitingListsDataGridView : UserControl
    {
        #region Fields

        private string firstSort;

        private ListSortDirection lastDirection;

        private readonly List<DataGridRow> dataGridRowList = new List<DataGridRow>();

        #endregion

        #region Constructors

        public WaitingListsDataGridView()
        {
            this.InitializeComponent();
            this.firstSort = "";
        }

        #endregion

        #region Events

        public event SelectionChangedEventHandler DataGridSelectionChanged
        {
            add { this.DataGrid.SelectionChanged += value; }
            remove { this.DataGrid.SelectionChanged -= value; }
        }

        #endregion

        #region Methods

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
            if (cView.SortDescriptions.Count >= 2
                || cView.SortDescriptions.Count == 0
                || e.Column.SortMemberPath == this.firstSort
                )
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
            e.Handled = true;
        }
        private static DependencyObject GetVisualParentOfType<T>(DependencyObject startObject)
        {
            DependencyObject parent = startObject;

            while (IsNotNullAndNotOfType<T>(parent))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent is T ? parent : null;
        }

        private static bool IsNotNullAndNotOfType<T>(DependencyObject obj)
        {
            return obj != null && !(obj is T);
        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                e.OriginalSource is DataGridRow row)
            {
                row.IsSelected = !row.IsSelected;
                e.Handled = true;
            }
        }

        private void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is FrameworkElement element
                && GetVisualParentOfType<DataGridRow>(element) is DataGridRow row
                && GetVisualParentOfType<DataGrid>(element) is DataGrid gridParent)
                {
                    row.IsSelected = !row.IsSelected;

                    if (row.IsSelected)
                    {
                        this.dataGridRowList.Add(row);
                    }
                    else
                    {
                        this.dataGridRowList.Remove(row);
                    }

                    var selectedList = gridParent.SelectedItems.OfType<ItemListExecution>().ToList();

                    if (selectedList.Exists(x => x.IsDispatchable == false))
                    {
                        foreach (var _row in this.dataGridRowList)
                        {
                            _row.IsSelected = false;
                        }

                        this.dataGridRowList.Clear();
                        this.dataGridRowList.Add(row);
                        row.IsSelected = true;

                    }

                    e.Handled = true;
                }
            }
            catch (System.Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion
    }
}
