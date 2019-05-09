using System.Windows.Input;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellsViewModel : EntityPagedListViewModel<Cell, int>
    {
        #region Methods

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.CELLDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
