using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemsViewModel : EntityListViewModel<Item>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            var id = (this.CurrentItem != null) ? ((Item)this.CurrentItem).Id : (int?)null;
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.ITEMDETAILS, id);
        }

        #endregion Methods
    }
}
