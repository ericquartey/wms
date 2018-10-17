using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : EntityListViewModel<LoadingUnit>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITDETAILS, this.CurrentItem?.Id);
        }

        #endregion Methods
    }
}
