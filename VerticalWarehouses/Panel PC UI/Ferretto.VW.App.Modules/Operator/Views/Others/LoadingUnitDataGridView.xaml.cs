using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class LoadingUnitDataGridView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable<LoadingUnit>),
            typeof(LoadingUnitDataGridView),
            new PropertyMetadata(OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(LoadingUnit),
            typeof(LoadingUnitDataGridView));

        private string firstSort;

        private string lastCode;

        private ListSortDirection lastDirection;

        private Timer reset;

        #endregion

        #region Constructors

        public LoadingUnitDataGridView()
        {
            this.InitializeComponent();
            this.DataGrid.DataContext = this;
            this.firstSort = string.Empty;
            this.lastCode = string.Empty;

            this.reset = new Timer(500);
            this.reset.Elapsed += new ElapsedEventHandler(this.OnTimerElapsed);
            this.reset.AutoReset = true;
        }

        #endregion

        #region Properties

        public IEnumerable<LoadingUnit> ItemsSource
        {
            get => (IEnumerable<LoadingUnit>)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public LoadingUnit SelectedItem
        {
            get => (LoadingUnit)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
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

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var istance = d as LoadingUnitDataGridView;
            var selectedItem = istance.DataGrid.SelectedItem as LoadingUnit;

            if (selectedItem != null)
            {
                istance.lastCode = selectedItem.Code;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.lastCode))
            {
                var items = this.DataGrid.ItemsSource as List<LoadingUnit>;
                this.DataGrid.SelectedItem = items.FirstOrDefault(s => s.Code == this.lastCode);

                if (!this.reset.Enabled)
                {
                    this.reset.Enabled = true;
                }
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

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.lastCode = string.Empty;
        }

        #endregion
    }
}
