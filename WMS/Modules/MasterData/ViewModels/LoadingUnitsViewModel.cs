using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : EntityListViewModel<LoadingUnit>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            var id = (this.CurrentItem != null) ? ((LoadingUnit)this.CurrentItem).Id : (int?)null;
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITDETAILS, id);
        }

        #endregion Methods
    }
}
