using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentsViewModel : EntityListViewModel<Compartment>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            var id = (this.CurrentItem != null) ? ((Compartment)this.CurrentItem).Id : (int?)null;
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.MasterData), Common.Utils.Modules.MasterData.COMPARTMENTDETAILS, id);
        }

        #endregion Methods
    }
}
