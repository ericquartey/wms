using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CellsViewModel : EntityListViewModel<Cell>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.CELLDETAILS, this.CurrentItem?.Id);
        }

        #endregion Methods
    }
}
