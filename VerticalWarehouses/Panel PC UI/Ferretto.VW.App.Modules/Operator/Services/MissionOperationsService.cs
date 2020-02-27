using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator
{
    internal sealed class MissionOperationsService : IMissionOperationsService
    {
        #region Fields

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IMachineMissionsWebService missionsWebService;

        private readonly IOperatorHubClient operatorHubClient;

        private SubscriptionToken healthToken;

        private SubscriptionToken loadingUnitToken;

        #endregion

        #region Constructors

        public MissionOperationsService(
            IMachineMissionOperationsWebService missionOperationsWebService,
            IMachineMissionsWebService missionsWebService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IEventAggregator eventAggregator,
            IOperatorHubClient operatorHubClient)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionsWebService = missionsWebService ?? throw new ArgumentNullException(nameof(missionsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));

            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.operatorHubClient.AssignedMissionChanged += async (sender, e) => await this.OnAssignedMissionChangedAsync(sender, e);
            this.operatorHubClient.AssignedMissionOperationChanged += async (sender, e) => await this.OnAssignedMissionOperationChangedAsync(sender, e);

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Properties

        public Mission ActiveMachineMission { get; private set; }

        public MissionWithLoadingUnitDetails ActiveWmsMission { get; private set; }

        public MissionOperation ActiveWmsOperation { get; private set; }

        #endregion

        #region Methods

        public async Task CompleteCurrentAsync(double quantity)
        {
            await this.missionOperationsWebService.CompleteAsync(
                this.ActiveWmsOperation.Id,
                quantity,
                ConfigurationManager.AppSettings.GetLabelPrinterName());

            await this.RefreshActiveMissionAsync();
        }

        public async Task PartiallyCompleteCurrentAsync(double quantity)
        {
            await this.missionOperationsWebService.PartiallyCompleteAsync(
                this.ActiveWmsOperation.Id,
                quantity,
                ConfigurationManager.AppSettings.GetLabelPrinterName());

            await this.RefreshActiveMissionAsync();
        }

        public async Task RecallLoadingUnitAsync(int id)
        {
            await this.loadingUnitsWebService.RemoveFromBayAsync(id);

            this.ActiveMachineMission = null;
            this.ActiveWmsMission = null;
            this.ActiveWmsOperation = null;

            this.RaiseMissionChangedEvent();
        }

        public async Task StartAsync()
        {
            this.loadingUnitToken = this.loadingUnitToken
                ??
                this.eventAggregator
                    .GetEvent<NotificationEventUI<MoveLoadingUnitMessageData>>()
                    .Subscribe(
                        async e => await this.OnLoadingUnitMovedAsync(e),
                        ThreadOption.BackgroundThread,
                        false);

            this.healthToken = this.healthToken
                ??
                this.eventAggregator.GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                   .Subscribe(
                        async e => await this.OnHealthStatusChangedAsync(e),
                        ThreadOption.BackgroundThread,
                        false);

            await this.RefreshActiveMissionAsync();
        }

        private async Task OnAssignedMissionChangedAsync(object sender, AssignedMissionChangedEventArgs e)
        {
            if (e.BayNumber == this.bayNumber)
            {
                this.logger.Debug($"Mission assigned to bay has changed to '{e.MissionId}'.");
                await this.RefreshActiveMissionAsync();
            }
        }

        private async Task OnAssignedMissionOperationChangedAsync(object sender, AssignedMissionOperationChangedEventArgs e)
        {
            if (e.BayNumber == this.bayNumber)
            {
                this.logger.Debug($"New mission operations available.");
                await this.RefreshActiveMissionAsync();
            }
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if ((e.HealthWmsStatus is HealthStatus.Healthy || e.HealthWmsStatus is HealthStatus.Degraded)
                &&
                (e.HealthMasStatus is HealthStatus.Healthy || e.HealthMasStatus is HealthStatus.Degraded))
            {
                this.logger.Debug($"Health status of services has changed.");

                await this.RefreshActiveMissionAsync();
            }
        }

        private async Task OnLoadingUnitMovedAsync(NotificationMessageUI<MoveLoadingUnitMessageData> message)
        {
            if (message.Data.MissionType is CommonUtils.Messages.Enumerations.MissionType.OUT
               &&
               message.Status is CommonUtils.Messages.Enumerations.MessageStatus.OperationWaitResume)
            {
                try
                {
                    this.logger.Debug($"Outgoing loading unit is waiting for an operation.");

                    this.ActiveMachineMission = await this.RetrieveActiveMissionAsync();
                    this.ActiveWmsMission = null;
                    this.ActiveWmsOperation = null;

                    this.RaiseMissionChangedEvent();
                }
                catch
                {
                    // do nothing
                }
            }
        }

        private void RaiseMissionChangedEvent()
        {
            this.logger.Debug($"Notifying mission {this.ActiveMachineMission?.Id}, WMS mission {this.ActiveWmsMission?.Id}, operation {this.ActiveWmsOperation?.Id}.");

            this.eventAggregator
               .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
               .Publish(new MissionChangedEventArgs(this.ActiveMachineMission, this.ActiveWmsMission, this.ActiveWmsOperation));
        }

        private async Task RefreshActiveMissionAsync()
        {
            var newMachineMission = await this.RetrieveActiveMissionAsync();
            MissionWithLoadingUnitDetails newWmsMission = null;
            MissionOperation newWmsOperation = null;
            MissionOperationInfo newWmsOperationInfo = null;

            try
            {
                if (newMachineMission != null && newMachineMission.WmsId.HasValue)
                {
                    this.logger.Debug($"Active mission has WMS id {newMachineMission.WmsId}.");

                    newWmsMission = await this.missionsWebService.GetWmsDetailsByIdAsync(newMachineMission.WmsId.Value);

                    var sortedOperations = newWmsMission.Operations.OrderBy(o => o.Priority).ThenBy(o => o.CreationDate);

                    newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.Executing);
                    if (newWmsOperationInfo is null)
                    {
                        newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.New);
                    }

                    if (newWmsOperationInfo is null)
                    {
                        this.logger.Debug($"Active WMS mission {newMachineMission.WmsId} has no executable mission operation.");

                        await this.loadingUnitsWebService.ResumeWmsAsync(newMachineMission.LoadUnitId, newMachineMission.Id);

                        newMachineMission = null;
                        newWmsMission = null;
                    }
                    else
                    {
                        this.logger.Debug($"Active mission has WMS operation {newWmsOperationInfo.Id}.");
                        newWmsOperation = await this.missionOperationsWebService.GetByIdAsync(newWmsOperationInfo.Id);

                        await this.missionOperationsWebService.ExecuteAsync(newWmsOperationInfo.Id);
                    }
                }

                if (newMachineMission?.Id != this.ActiveMachineMission?.Id
                   ||
                   newWmsMission?.Id != this.ActiveWmsMission?.Id
                   ||
                   newWmsOperation?.Id != this.ActiveWmsOperation?.Id
                   ||
                   newWmsOperation?.RequestedQuantity != this.ActiveWmsOperation?.RequestedQuantity
                   ||
                   newWmsOperation?.DispatchedQuantity != this.ActiveWmsOperation?.DispatchedQuantity
                   ||
                   (newWmsMission != null && this.ActiveWmsMission?.Operations.Any(mo => newWmsMission.Operations.Any(nOp => nOp.Id != mo.Id)) == true))
                {
                    this.ActiveMachineMission = newMachineMission;
                    this.ActiveWmsMission = newWmsMission;
                    this.ActiveWmsOperation = newWmsOperation;

                    this.RaiseMissionChangedEvent();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task<Mission> RetrieveActiveMissionAsync()
        {
            this.logger.Debug("Retrieving active mission ...");

            try
            {
                var machineMissions = await this.missionsWebService.GetAllAsync();

                var activeMissions = machineMissions.Where(m =>
                    m.Step is MissionStep.WaitPick
                    &&
                    m.TargetBay == this.bayNumber)
                    .OrderBy(o => o.LoadUnitDestination);

                this.logger.Debug(!activeMissions.Any()
                    ? "No active mission on bay."
                    : $"Active mission has id {activeMissions.FirstOrDefault().Id}.");

                return activeMissions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return null;
        }

        #endregion
    }
}
