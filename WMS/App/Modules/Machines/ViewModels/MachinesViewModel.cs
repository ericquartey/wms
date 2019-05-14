using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.Machines
{
    public class MachinesViewModel : EntityPagedListViewModel<Machine, int>
    {
        #region Constructors

        public MachinesViewModel(IDataSourceService dataSourceService)
          : base(dataSourceService)
        {
            this.machineStatusEventSubscription = this.EventService.Subscribe<MachineStatusPubSubEvent>(
                this.OnMachineStatusChanged,
                this.Token,
                keepSubscriberReferenceAlive: true,
                forceUiThread: true);
        }
        {
            this.FlattenDataSource = true;
        }

        #endregion
    }
}
