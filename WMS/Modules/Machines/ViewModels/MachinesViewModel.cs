using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Machines
{
    public class MachinesViewModel : EntityListViewModel<Machine, int>
    {
        #region Methods

        public override void ExecuteViewDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Common.Utils.Modules.Machines), Common.Utils.Modules.Machines.MACHINEDETAILS, this.CurrentItem?.Id);
        }

        #endregion Methods
    }
}
