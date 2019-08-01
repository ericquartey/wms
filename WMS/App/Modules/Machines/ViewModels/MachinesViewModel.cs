using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Modules.BLL;

namespace Ferretto.WMS.App.Modules.Machines
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Machine), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.LoadingUnit), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    public class MachinesViewModel : EntityListViewModel<Machine, int>
    {
        #region Fields

        private readonly object machineStatusEventSubscription;

        #endregion

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

        #endregion

        #region Methods

        protected override async Task LoadDataAsync(ModelChangedPubSubEvent e)
        {
            if (this.SelectedFilterDataSource is DataSourceCollection<Machine, int> enumerableSource)
            {
                this.IsBusy = true;
                await enumerableSource.RefreshAsync();
                this.IsBusy = false;
            }
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<MachineStatusPubSubEvent>(
                this.machineStatusEventSubscription);

            base.OnDispose();
        }

        private void OnMachineStatusChanged(MachineStatusPubSubEvent e)
        {
            if (e == null)
            {
                return;
            }

            if (this.SelectedFilterDataSource is IEnumerable<Machine> machines)
            {
                var machine = machines.SingleOrDefault(m => m.Id == e.MachineStatus.MachineId);
                if (machine != null)
                {
                    machine.Status = (MachineStatus)e.MachineStatus.Mode;
                }
            }
        }

        #endregion
    }
}
