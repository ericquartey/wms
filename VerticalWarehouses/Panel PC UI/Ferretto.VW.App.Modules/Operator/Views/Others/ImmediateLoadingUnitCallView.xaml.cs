using System.Linq;
using System.Windows.Controls;
using Ferretto.VW.App.Modules.Operator.ViewModels;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Views
{
    public partial class ImmediateLoadingUnitCallView
    {
        #region Constructors

        public ImmediateLoadingUnitCallView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void CellsDataGrid_DataGridSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as ImmediateLoadingUnitCallViewModel;
            var grid = sender as DataGrid;

            if (grid.SelectedItems != null)
            {
                var selectedList = grid.SelectedItems.OfType<LoadingUnit>().ToList();

                viewModel.SelectedUnits = selectedList;

                var template = grid.Template;
                var scrol = (ScrollViewer)template.FindName("DG_ScrollViewer", grid);

                if (scrol.VerticalOffset + 10 <= grid.SelectedIndex || scrol.VerticalOffset > grid.SelectedIndex)
                {
                    scrol.ScrollToVerticalOffset(grid.SelectedIndex);
                }
            }
        }

        #endregion
    }
}
