using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Modules.BLL;

namespace Ferretto.WMS.Modules.Machines
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Machine))]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.LoadingUnit), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Cell), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Mission), false)]
    public class MachineDetailsViewModel : DetailsViewModel<MachineDetails>, IEdit
    {
        #region Fields

        private readonly IMachineProvider machineProvider = ServiceLocator.Current.GetInstance<IMachineProvider>();

        private readonly object machineStatusEventSubscription;

        private Collection<IMapModel> dataChart;

        private MachineLive machineLive;

        private BayDetails selectedBay;

        private object selectedFilterDataSource;

        #endregion

        #region Constructors

        public MachineDetailsViewModel()
        {
            this.machineStatusEventSubscription = this.EventService.Subscribe<MachineStatusPubSubEvent>(
                this.OnMachineStatusChanged,
                this.Token,
                keepSubscriberReferenceAlive: true,
                forceUiThread: true);
        }

        #endregion

        #region Properties

        public Collection<IMapModel> DataChart { get => this.dataChart; set => this.SetProperty(ref this.dataChart, value); }

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

                    this.DataChart = new Collection<IMapModel>
                    {
                        new MapModel(nameof(this.Model.ManualTime), this.Model.ManualTime),
                        new MapModel(nameof(this.Model.MissionTime), this.Model.MissionTime),
                        new MapModel(nameof(this.Model.AutomaticTime), this.Model.AutomaticTime),
                        new MapModel(nameof(this.Model.ErrorTime), this.Model.ErrorTime),
                    };
                }
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
            }
            finally
            {
                this.IsBusy = false;
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
                NetWeight = this.Model.NetWeight,
                GrossWeight = this.Model.GrossWeight,
            };
            this.MachineLive.CalculateWeightFillRate();
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
