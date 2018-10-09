using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemsViewModel : EntityListViewModel<Item, int>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.ITEMDETAILS, this.CurrentItem?.Id);
        }

        #endregion Methods
    }
}
