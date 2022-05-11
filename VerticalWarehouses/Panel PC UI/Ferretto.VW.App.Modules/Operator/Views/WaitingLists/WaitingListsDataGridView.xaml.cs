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

        private readonly List<DataGridRow> dataGridRowList = new List<DataGridRow>();

        #endregion

        #region Constructors

        public WaitingListsDataGridView()
        {
            this.InitializeComponent();
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

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataGrid.SelectedItems.Clear();
        }
    }
}
