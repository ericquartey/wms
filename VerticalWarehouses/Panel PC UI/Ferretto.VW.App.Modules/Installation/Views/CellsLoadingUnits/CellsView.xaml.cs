using System;
using System.Linq;
using System.Windows.Controls;
using Ferretto.VW.App.Installation.ViewModels;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.Views
{
    public partial class CellsView
    {
        #region Constructors

        public CellsView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void CellsDataGrid_DataGridSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as CellsViewModel;
            var grid = sender as DataGrid;

            if (grid.SelectedItems != null)
            {
                viewModel.SelectedCells = grid.SelectedItems.OfType<CellPlus>().ToList();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedBlockLevel = (sender as ComboBox).SelectedValue?.ToString();

                if (selectedBlockLevel == BlockLevel.Blocked.ToString())
                {
                    this.IsFreeCheckBox.IsChecked = false;
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
