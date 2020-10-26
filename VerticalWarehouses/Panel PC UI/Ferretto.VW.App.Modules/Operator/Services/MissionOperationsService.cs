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
    internal sealed class MissionOperationsService : IMissionOperationsService, IDisposable
    {
        #region Fields

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineAccessoriesWebService machineAccessoriesWebService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IMachineMissionsWebService missionsWebService;

        private readonly IOperatorHubClient operatorHubClient;

        private SubscriptionToken healthToken;

        private bool isDisposed;

        private bool isRecallUnit;

        private SubscriptionToken loadingUnitToken;

        private int unitId;

        #endregion

        #region Constructors

        public MissionOperationsService(
            IMachineMissionOperationsWebService missionOperationsWebService,
            IMachineMissionsWebService missionsWebService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineAccessoriesWebService machineAccessoriesWebService,
            IMachineBaysWebService machineBaysWebService,
            IEventAggregator eventAggregator,
            IOperatorHubClient operatorHubClient)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionsWebService = missionsWebService ?? throw new ArgumentNullException(nameof(missionsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));
            this.machineAccessoriesWebService = machineAccessoriesWebService ?? throw new ArgumentNullException(nameof(machineAccessoriesWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
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

        public async Task<bool> CompleteAsync(int operationId, double quantity, string barcode = null)
        {
            this.logger.Debug($"User requested to complete operation '{operationId}'.");

            var operationToComplete = await this.missionOperationsWebService.GetByIdAsync(operationId);
            this.logger.Debug($"Operation to complete has status '{operationToComplete.Status}'.");

            if (operationToComplete.Status is MissionOperationStatus.Executing)
            {
                var labelPrinterName = await this.GetLabelPrinterNameAsync();
                await this.missionOperationsWebService.CompleteAsync(
                    operationId,
                    quantity,
                    labelPrinterName,
                    barcode);

                await this.RefreshActiveMissionAsync();

                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.healthToken?.Dispose();
                this.healthToken = null;

                this.loadingUnitToken?.Dispose();
                this.healthToken = null;

                this.isDisposed = true;
            }
        }

        public bool IsRecallLoadingUnitId()
        {
            return this.isRecallUnit;
        }

        public async Task<bool> PartiallyCompleteAsync(int operationId, double quantity)
        {
            this.logger.Debug($"User requested to partially complete operation '{operationId}' with quantity {quantity}.");
            var operationToComplete = await this.missionOperationsWebService.GetByIdAsync(operationId);

            if (operationToComplete.Status is MissionOperationStatus.Executing)
            {
                var labelPrinterName = await this.GetLabelPrinterNameAsync();
                await this.missionOperationsWebService.PartiallyCompleteAsync(
                    operationId,
                    quantity,
                    labelPrinterName);

                await this.RefreshActiveMissionAsync();

                return true;
            }

            return false;
        }

        public async Task RecallLoadingUnitAsync(int id)
        {
            await this.loadingUnitsWebService.RemoveFromBayAsync(id);

            this.unitId = id;
            this.isRecallUnit = true;

            this.ActiveMachineMission = null;
            this.ActiveWmsMission = null;
            this.ActiveWmsOperation = null;

            this.RaiseMissionChangedEvent();
        }

        public int RecallLoadingUnitId()
        {
            return this.unitId;
        }

        public async Task RefreshAsync()
        {
            await this.RefreshActiveMissionAsync();
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

        private async Task<string> GetLabelPrinterNameAsync()
        {
            var accessories = await this.machineAccessoriesWebService.GetAllAsync();
            if (!accessories.LabelPrinter.IsEnabledNew)
            {
                return null;
            }

            return accessories.LabelPrinter.Name;
        }

        private async Task OnAssignedMissionChangedAsync(object sender, AssignedMissionChangedEventArgs e)
        {
            if (e.BayNumber == this.bayNumber)
            {
                this.logger.Debug($"Mission assigned to bay has changed to '{e.MissionId}'.");
                this.ActiveWmsMission = null;
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
            try
            {
                var newMachineMission = await this.RetrieveActiveMissionAsync();
                MissionWithLoadingUnitDetails newWmsMission = null;
                MissionOperation newWmsOperation = null;
                MissionOperationInfo newWmsOperationInfo = null;

                if (newMachineMission != null && newMachineMission.WmsId.HasValue)
                {
                    this.logger.Debug($"Active mission has WMS id '{newMachineMission.WmsId}'.");

                    newWmsMission = await this.missionsWebService.GetWmsDetailsByIdAsync(newMachineMission.WmsId.Value);

                    var sortedOperations = newWmsMission.Operations.OrderBy(o => o.Priority)
                                                                   .ThenBy(o => (o.Status is MissionOperationStatus.Completed ? 0 : newWmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == o.CompartmentId)?.XPosition))
                                                                   .ThenBy(o => (o.Status is MissionOperationStatus.Completed ? 0 : newWmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == o.CompartmentId)?.YPosition))
                                                                   .ThenBy(o => o.CreationDate);

                    newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.Executing);
                    if (newWmsOperationInfo is null)
                    {
                        newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.New);
                    }

                    if (newWmsOperationInfo is null)
                    {
                        this.logger.Debug($"Active WMS mission '{newMachineMission.WmsId}' has no executable mission operation.");

                        this.logger.Debug($"Recalling loading unit '{newMachineMission.LoadUnitId}'.");

                        await this.loadingUnitsWebService.RemoveFromBayAsync(newMachineMission.LoadUnitId);

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
                else
                {
                    this.logger.Trace($"No Active mission.");
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
                    // if (this.ActiveMachineMission?.LoadUnitId != null
                    //    &&
                    //    this.ActiveMachineMission?.LoadUnitId != newMachineMission?.LoadUnitId)
                    // {
                    //    this.logger.Debug($"Old WMS mission '{this.ActiveMachineMission.Id}' was removed, but must be completed before proceeding: recalling loading unit '{this.ActiveMachineMission.LoadUnitId}'.");
                    //    await this.loadingUnitsWebService.RemoveFromBayAsync(this.ActiveMachineMission.LoadUnitId);
                    // }
                    this.ActiveMachineMission = newMachineMission;
                    this.ActiveWmsMission = newWmsMission;
                    this.ActiveWmsOperation = newWmsOperation;

                    this.RaiseMissionChangedEvent();
                }
                else
                {
                    this.logger.Trace($"No mission changed.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task<Mission> RetrieveActiveMissionAsync()
        {
            this.logger.Trace("Retrieving active mission ...");

            try
            {
                // Retrieve properties of bay: check if it is an internal double bay
                var bay = await this.machineBaysWebService.GetByNumberAsync(this.bayNumber);
                var isInternalDoubleBay = bay.IsDouble && (bay.Carousel == null);

                // Retrieve the machine missions
                var machineMissions = await this.missionsWebService.GetAllAsync();

                // Retrieve the active missions in the given bay according to the bay's properties
                IOrderedEnumerable<Mission> activeMissions = null;
                if (isInternalDoubleBay == false)
                {
                    // Retrieve the active missions according to the enlisted condition.
                    // The missions are ordered by the location of destination for the load unit
                    activeMissions = machineMissions.Where(m =>
                    m.Step is MissionStep.WaitPick
                    &&
                    m.TargetBay == this.bayNumber
                    &&
                    m.Status == MissionStatus.Waiting)
                    .OrderBy(o => o.LoadUnitDestination);
                }
                else
                {
                    // Retrieve the active missions according to the enlisted condition.
                    // The missions are ordered by creation date (descending way)
                    activeMissions = machineMissions.Where(m =>
                    m.Step is MissionStep.WaitPick
                    &&
                    m.TargetBay == this.bayNumber
                    &&
                    m.Status == MissionStatus.Waiting)
                    .OrderByDescending(d => d.CreationDate);
                }

                if (activeMissions.Any())
                {
                    this.logger.Debug($"Active mission has id {activeMissions.FirstOrDefault().Id}");
                }
                else
                {
                    this.logger.Trace("No active mission on bay");
                }

                // Retrieve the first one
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
