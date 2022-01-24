using System.Linq;
using System.Windows.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Modules.Operator.ViewModels;
using System.Linq;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class WaitingListsView
    {
        #region Constructors

        public WaitingListsView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void CellsDataGrid_DataGridSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as WaitingListsViewModel;
            var grid = sender as DataGrid;

            if (grid.SelectedItems != null)
            {
                var selectedList = grid.SelectedItems.OfType<ItemListExecution>().ToList();

                if (selectedList.Exists(x=>x.IsDispatchable == false))
                {
                    viewModel.SelectedCells = selectedList.FindAll(x=>x.IsDispatchable == false);
                }
                else
                {
                    viewModel.SelectedCells = selectedList;
                }
            }
        }

        #endregion
    }
}
