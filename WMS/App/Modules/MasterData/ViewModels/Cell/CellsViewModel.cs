using System.Windows.Input;
using Ferretto.Common.Controls;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellsViewModel : EntityPagedListViewModel<Cell, int>
    {
        #region Fields

        private ICommand showCellDetailsCommand;

        #endregion

        #region Properties

        public ICommand ShowCellDetailsCommand => this.showCellDetailsCommand ??
            (this.showCellDetailsCommand = new DelegateCommand(
                    this.ShowCellDetails,
                    this.CanShowCellDetails)
            .ObservesProperty(() => this.CurrentItem));

        #endregion

        #region Methods

        private bool CanShowCellDetails()
        {
            return this.CurrentItem != null;
        }

        private void ShowCellDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.CELLDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
