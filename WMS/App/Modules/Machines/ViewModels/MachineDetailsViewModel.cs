using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Modules.BLL;

namespace Ferretto.WMS.App.Modules.Machines
{
    public class MachineDetailsViewModel : DetailsViewModel<MachineDetails>, IEdit
    {
        #region Fields

        private readonly IMachineProvider machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

        private readonly object machineStatusEventSubscription;

        private MachineLive machineLive;

        private BayDetails selectedBay;

        private object selectedFilterDataSource;

        #endregion

        #region Constructors

        public MachineDetailsViewModel(IDataSourceService dataSourceService)
        {
            this.machineStatusEventSubscription = this.EventService.Subscribe<MachineStatusPubSubEvent>(
                this.OnMachineStatusChanged,
                this.Token,
                keepSubscriberReferenceAlive: true,
                forceUiThread: true);
        }

        #endregion

        #region Properties

        public static ModelObject CellObject => ModelObject.Cell;

        public static ModelObject CompartmentObject => ModelObject.Compartment;

        public static ModelObject ItemListObject => ModelObject.ItemList;

        public static ModelObject ItemListRowObject => ModelObject.ItemListRow;

        public static ModelObject ItemObject => ModelObject.Item;

        public static ModelObject LoadingUnitObject => ModelObject.LoadingUnit;

        public static ModelObject MissionObject => ModelObject.Mission;

        public static ModelObject SchedulerRequestObject => ModelObject.SchedulerRequest;

        public MachineLive MachineLive
        {
            get => this.machineLive; set => this.SetProperty(ref this.machineLive, value);
        }

        public BayDetails SelectedBay
        {
            get => this.selectedBay;
            set => this.SetProperty(ref this.selectedBay, value);
        }

        public virtual object SelectedFilterDataSource
        {
            get => this.selectedFilterDataSource;
            protected set
            {
                if (this.SetProperty(ref this.selectedFilterDataSource, value)
                    &&
                    this.selectedFilterDataSource is IRefreshableDataSource refreshableSource)
                {
                    refreshableSource.RefreshAsync();
                }
            }
        }

        #endregion

        #region Methods

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override Task ExecuteRevertCommandAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

                if (this.Data is int modelId)
                {
                    this.Model = await this.machineProvider.GetByIdAsync(modelId);
                    this.InitializeLiveData();
                    if (this.SelectedFilterDataSource is DataSourceCollection<Bay, int> enumerableSource)
                    {
                        await enumerableSource.RefreshAsync();
                    }
                }

                this.IsBusy = false;
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
            }
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<MachineStatusPubSubEvent>(
                this.machineStatusEventSubscription);

            base.OnDispose();
        }

        private void InitializeLiveData()
        {
            this.MachineLive = new MachineLive
            {
                Bays = this.Model.Bays,
                Id = this.Model.Id,
                Status = this.Model.Status,
            };
        }

        private void OnMachineStatusChanged(MachineStatusPubSubEvent e)
        {
            if (e == null)
            {
                return;
            }

            if (this.machineLive is MachineLive machine)
            {
                machine.Status = (MachineStatus)e.MachineStatus.Mode;
                machine.FaultCode = e.MachineStatus.FaultCode;
                machine.CurrentLoadingUnitPosition = e.MachineStatus.ElevatorStatus.Position;
                machine.CurrentLoadingUnitId = e.MachineStatus.ElevatorStatus.LoadingUnitId;

                foreach (var bay in machine.Bays)
                {
                    var bayStatus = e.MachineStatus.BaysStatus.SingleOrDefault(b => b.BayId == bay.Id);
                    if (bayStatus != null)
                    {
                        bay.IsActive = bayStatus.LoggedUserId != null;
                        bay.LoadingUnitInBayId = bayStatus.LoadingUnitId;
                        bay.UserLogged = bayStatus.LoggedUserId;
                    }
                }
            }
        }

        #endregion
    }
}
