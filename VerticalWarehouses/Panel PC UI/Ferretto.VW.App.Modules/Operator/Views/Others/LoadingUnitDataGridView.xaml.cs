using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class LoadingUnitDataGridView : UserControl
    {
        #region Fields

        private string firstSort;

        private ListSortDirection lastDirection;

        #endregion

        #region Constructors

        public LoadingUnitDataGridView()
        {
            this.InitializeComponent();
            this.firstSort = string.Empty;
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

        private static void AddSortColumn(DataGrid sender, string sortColumn, ListSortDirection direction)
        {
            var cView = CollectionViewSource.GetDefaultView(sender.ItemsSource);
            cView.SortDescriptions.Add(new SortDescription(sortColumn, direction));

            // Add the sort arrow on the DataGridColumn
            foreach (var col in sender.Columns.Where(x => x.SortMemberPath == sortColumn))
            {
                col.SortDirection = direction;
            }
        }

        private void DataGridName_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var dgSender = (DataGrid)sender;
            var cView = CollectionViewSource.GetDefaultView(dgSender.ItemsSource);

            // Alternate between ascending/descending if the same column is clicked
            ListSortDirection direction = ListSortDirection.Ascending;
            if (cView.SortDescriptions.FirstOrDefault().PropertyName == e.Column.SortMemberPath)
            {
                direction = cView.SortDescriptions.FirstOrDefault().Direction == ListSortDirection.Descending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            }

            // To this point the default sort functionality is implemented
            // Now check the wanted columns and add multiple sort
            if (cView.SortDescriptions.Count >= 2 || cView.SortDescriptions.Count == 0)
            {
                this.lastDirection = direction;
                cView.SortDescriptions.Clear();
                AddSortColumn((DataGrid)sender, e.Column.SortMemberPath, direction);
            }
            else if (e.Column.SortMemberPath != this.firstSort)
            {
                this.firstSort = e.Column.SortMemberPath;
                this.lastDirection = direction;
                AddSortColumn((DataGrid)sender, this.firstSort, direction);
            }
            else if (e.Column.SortMemberPath == this.firstSort && this.lastDirection != direction)
            {
                this.lastDirection = direction;
                cView.SortDescriptions.Clear();
                AddSortColumn((DataGrid)sender, e.Column.SortMemberPath, direction);
            }

            e.Handled = true;
        }

        #endregion
    }
}
