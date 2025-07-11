﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
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

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly IMachineAreasWebService areasWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly BayNumber bayNumber;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineItemListsWebService itemListsWebService;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineAccessoriesWebService machineAccessoriesWebService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineService machineService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly IMachineMissionsWebService missionsWebService;

        private readonly IOperatorHubClient operatorHubClient;

        private readonly ISessionService sessionService;

        private SubscriptionToken healthToken;

        private bool isDisposed;

        private bool isRecallUnit;

        private SubscriptionToken loadingUnitToken;

        private IEnumerable<Mission> machineMissions;

        private bool multilist;

        private int unitId;

        #endregion

        #region Constructors

        public MissionOperationsService(
            IMachineMissionOperationsWebService missionOperationsWebService,
            IMachineMissionsWebService missionsWebService,
            IMachineItemListsWebService itemListsWebService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineAccessoriesWebService machineAccessoriesWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineService machineService,
            IEventAggregator eventAggregator,
            IOperatorHubClient operatorHubClient,
            IAuthenticationService authenticationService,
            IMachineAreasWebService areasWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IBayManager bayManager)
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionsWebService = missionsWebService ?? throw new ArgumentNullException(nameof(missionsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineAccessoriesWebService = machineAccessoriesWebService ?? throw new ArgumentNullException(nameof(machineAccessoriesWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.areasWebService = areasWebService ?? throw new ArgumentNullException(nameof(areasWebService));
            this.itemListsWebService = itemListsWebService ?? throw new ArgumentNullException(nameof(itemListsWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            this.operatorHubClient.AssignedMissionChanged += async (sender, e) => await this.OnAssignedMissionChangedAsync(sender, e);
            this.operatorHubClient.AssignedMissionOperationChanged += async (sender, e) => await this.OnAssignedMissionOperationChangedAsync(sender, e);

            this.bayNumber = ConfigurationManager.AppSettings.GetBayNumber();
        }

        #endregion

        #region Properties

        public Mission ActiveMachineMission { get; private set; }

        public MissionWithLoadingUnitDetails ActiveWmsMission { get; private set; }

        public MissionOperation ActiveWmsOperation { get; private set; }

        public int CurrentOperation { get; private set; } = 1;

        public int MaxOperation { get; private set; }

        #endregion

        #region Methods

        public async Task<bool> CompleteAsync(int operationId, double quantity, string barcode = null, double wastedQuantity = 0, string toteBarcode = null, int? nrLabels = null)
        {
            this.logger.Debug($"User requested to complete operation '{operationId}' with quantity {quantity}.");

            var operationToComplete = await this.missionOperationsWebService.GetByIdAsync(operationId);

            this.logger.Debug($"Operation to complete has status '{operationToComplete.Status}'.");

            if (operationToComplete.Status is MissionOperationStatus.Executing)
            {
                var labelPrinterName = await this.GetLabelPrinterNameAsync();
                await this.missionOperationsWebService.CompleteAsync(
                    operationId,
                    quantity,
                    labelPrinterName,
                    barcode,
                    wastedQuantity,
                    toteBarcode,
                    this.authenticationService.UserName,
                    nrLabels);

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

        public async Task<IEnumerable<ItemList>> GetAllMissionsMachineAsync()
        {
            var machine = this.sessionService.MachineIdentity;
            var list = await this.areasWebService.GetItemListsAsync(machine.AreaId.Value, machine.Id, (int)this.bayNumber, true, this.authenticationService.UserName);

            return list;
        }

        public async Task<IEnumerable<ProductInMachine>> GetProductsAsync(int? areaId, string itemCode, CancellationToken? cancellationToken = null)
        {
            var isLocal = await this.missionsWebService.IsLocalMachineItemsAsync();
            if (isLocal)
            {
                if (cancellationToken.HasValue)
                {
                    return await this.areasWebService.GetProductsAsync(
                            areaId.Value,
                            0,
                            0,
                            itemCode,
                            false,
                            true,
                            cancellationToken.Value);
                }
                else
                {
                    return await this.areasWebService.GetProductsAsync(
                            areaId.Value,
                            0,
                            1,
                            itemCode,
                            false,
                            false);
                }
            }
            else
            {
                if (cancellationToken.HasValue)
                {
                    return await this.areasWebService.GetAllProductsAsync(itemCode, cancellationToken.Value);
                }
                else
                {
                    return await this.areasWebService.GetAllProductsAsync(itemCode);
                }
            }
        }

        public async Task<bool> IsLastRowForListAsync(string itemListCode)
        {
            var bay = this.machineService.Bay;
            var machineIdentity = this.sessionService.MachineIdentity;

            var machineId = machineIdentity.Id;
            var areaId = machineIdentity.AreaId;

            var fullLists = await this.areasWebService.GetItemListsAsync(areaId.Value, machineId, bay.Id, true, this.authenticationService.UserName);

            var activList = fullLists.ToList().Find(l => l.Code == itemListCode);

            if (activList != null)
            {
                var listRows = await this.itemListsWebService.GetRowsAsync(activList.Id);

                return listRows.Count(r => r.Machines.Any()) == 1;
            }

            return false;
        }

        public async Task<bool> IsLastWmsMissionForCurrentLoadingUnitAsync(int missionId)
        {
            var retValue = true;
            try
            {
                var newMachineMission = await this.RetrieveActiveMissionAsync();
                MissionWithLoadingUnitDetails newWmsMission = null;
                MissionOperationInfo newWmsOperationInfo = null;

                if (newMachineMission != null && newMachineMission.WmsId.HasValue)
                {
                    this.logger.Debug($"Active mission has WMS id '{newMachineMission.WmsId}'.");

                    newWmsMission = await this.missionsWebService.GetWmsDetailsByIdAsync(newMachineMission.WmsId.Value);

                    var sortedOperations = newWmsMission.Operations;

                    newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.Executing);

                    // detect if exists more executing mission except the specified one
                    if (sortedOperations.Any(o => o.Status is MissionOperationStatus.Executing))
                    {
                        var detected = false;
                        foreach (var op in sortedOperations.Where(o => o.Status is MissionOperationStatus.Executing && o.RequestedQuantity > 0))
                        {
                            if (op.Id != missionId)
                            {
                                // at least one different mission exists
                                detected = true;
                                break;
                            }
                        }

                        if (!detected)
                        {
                            // make null the WmsOperation info, the only executing mission is the specified one
                            newWmsOperationInfo = null;
                        }
                    }

                    if (newWmsOperationInfo is null)
                    {
                        newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.New);
                    }

                    if (newWmsOperationInfo is null)
                    {
                        this.logger.Debug($"Active WMS mission '{newMachineMission.WmsId}' has no executable mission operation.");

                        retValue = true;
                    }
                    else
                    {
                        this.logger.Debug($"Active mission has WMS operation {newWmsOperationInfo.Id}.");

                        retValue = false;
                    }

                    newMachineMission = null;
                    newWmsMission = null;
                }
                else
                {
                    this.logger.Trace($"No Active mission.");

                    if (this.ActiveMachineMission?.LoadUnitId != null)
                    {
                        var machineMissions = await this.missionsWebService.GetAllAsync();
                        if (this.ActiveMachineMission?.LoadUnitId != null &&
                            !machineMissions.Any(m =>
                                    m.LoadUnitId == this.ActiveMachineMission?.LoadUnitId &&
                                    m.TargetBay == this.bayNumber &&
                                    (m.Status == MissionStatus.Waiting || m.Status == MissionStatus.Executing || m.Status == MissionStatus.New)))
                        {
                            this.logger.Debug($"Old WMS mission '{this.ActiveMachineMission?.Id}' was removed, but must be completed before proceeding: removing loading unit '{this.ActiveMachineMission?.LoadUnitId}'.");

                            retValue = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return retValue;
        }

        public async Task<string> IsMultiMachineAsync(int missionId)
        {
            var machineList = string.Empty;
            try
            {
                var bay = this.machineService.Bay;

                if (bay.CheckListContinueInOtherMachine is false)
                {
                    return machineList;
                }
                var machine = this.sessionService.MachineIdentity;

                var allMissionsList = await this.areasWebService.GetItemListsAsync(machine.AreaId.Value, machine.Id, bay.Id, true, this.authenticationService.UserName);

                var currentMission = allMissionsList.ToList().Find(x => x.Code == this.ActiveWmsMission?.Operations.FirstOrDefault().ItemListCode);
                if (currentMission is null)
                {
                    return machineList;
                }
                var retVal = currentMission.Machines.ToList().Exists(x => x.Id != machine.Id);
                if (retVal)
                {
                    foreach (var otherMachine in currentMission.Machines.Where(x => x.Id != machine.Id))
                    {
                        machineList = machineList + " [" + otherMachine.Id.ToString() + "]";
                    }
                }
                return machineList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return machineList;
            }
        }

        public bool IsRecallLoadingUnitId()
        {
            return this.isRecallUnit;
        }

        public async Task<bool> MustCheckToteBarcode()
        {
            if (this.multilist)
            {
                return true;
            }

            var lists = await this.GetAllMissionsMachineAsync();
            this.multilist = lists != null
                && lists.Count(z => z.Status == ItemListStatus.Executing && z.ItemListType == ItemListType.Pick) > 1;
            return this.multilist;
        }

        public async Task<bool> PartiallyCompleteAsync(int operationId, double quantity, double wastedQuantity, string printerName, bool? emptyCompartment, bool? fullCompartment, int? nrLabels = null)
        {
            this.logger.Debug($"User requested to partially complete operation '{operationId}' with quantity {quantity}.");
            var operationToComplete = await this.missionOperationsWebService.GetByIdAsync(operationId);

            if (operationToComplete.Status is MissionOperationStatus.Executing)
            {
                var labelPrinterName = await this.GetLabelPrinterNameAsync();
                await this.missionOperationsWebService.PartiallyCompleteAsync(
                    operationId,
                    quantity,
                    wastedQuantity,
                    labelPrinterName,
                    emptyCompartment,
                    fullCompartment,
                    this.authenticationService.UserName,
                    nrLabels);

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

        public async Task<IEnumerable<Mission>> RefreshAsync(bool force = false)
        {
            await this.RefreshActiveMissionAsync(force: force);
            return this.machineMissions;
        }

        public async Task SetCurrentOperation(int value)
        {
            this.CurrentOperation = value;
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

        public async Task<bool> SuspendAsync(int operationId)
        {
            this.logger.Debug($"User requested to suspend operation '{operationId}'");

            var operationToSuspend = await this.missionOperationsWebService.GetByIdAsync(operationId);
            this.logger.Debug($"Operation to suspend has status '{operationToSuspend.Status}'.");

            if (operationToSuspend.Status is MissionOperationStatus.Executing)
            {
                try
                {
                    var operationSuspended = await this.missionOperationsWebService.SuspendAsync(operationId, this.authenticationService.UserName);
                    operationSuspended.Status = MissionOperationStatus.Suspended;

                    await this.RefreshActiveMissionAsync();

                    return true;
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.Message);
                }
            }

            return false;
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
                this.logger.Debug($"Mission assigned to bay has changed to id '{e.MissionId}'.");
                if (this.ActiveMachineMission?.Id != e?.MissionId)
                {
                    this.ActiveWmsMission = null;
                }
                await this.RefreshActiveMissionAsync(e.MissionId);
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

                    this.ActiveMachineMission = await this.RetrieveActiveMissionAsync(message.Data.MissionId);
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

        private async Task RefreshActiveMissionAsync(int? missionId = null, bool force = false)
        {
            // we only need one refresh at a time - if it is already active we can safely return
            if (await this._semaphore.WaitAsync(0))
            {
                try
                {
                    var newMachineMission = await this.RetrieveActiveMissionAsync(missionId);
                    MissionWithLoadingUnitDetails newWmsMission = null;
                    MissionOperation newWmsOperation = null;
                    MissionOperationInfo newWmsOperationInfo = null;

                    if (newMachineMission != null && newMachineMission.WmsId.HasValue)
                    {
                        this.logger.Debug($"Active mission has WMS id '{newMachineMission.WmsId}'.");

                        newWmsMission = await this.missionsWebService.GetWmsDetailsByIdAsync(newMachineMission.WmsId.Value);

                        var configuration = await this.machineConfigurationWebService.GetConfigAsync();
                        var OperationRightToLeft = configuration.OperationRightToLeft;

                        // Order the WMS operations:
                        // 1. By type, putting Pick and Put operations first
                        // 2. By ActiveWmsOperation.Id (if present), putting it first in the list
                        // 3. By RowSeq
                        // 4. By Priority
                        // 5. By CompartmentId (if a LoadingUnit is present), putting completed operations last
                        // 6. By XPosition within Compartment (if a LoadingUnit is present), putting completed operations last
                        // 7. By YPosition within Compartment (if a LoadingUnit is present), putting completed operations last
                        var sortedOperations = newWmsMission.Operations.OrderBy(o => o.Type is MissionOperationType.Pick || o.Type is MissionOperationType.Put ? 0 : 1)
                                //.ThenBy(o => (this.ActiveWmsOperation?.Id == 0 || o.Id == this.ActiveWmsOperation?.Id) ? 0 : 1)
                                .ThenBy(o => o.RowSeq)
                                .ThenBy(o => o.Priority)
                                //.ThenBy(o => o.Status is MissionOperationStatus.Completed || newWmsMission.LoadingUnit == null ? 0 : o.CompartmentId);
                                .ThenBy(o => newWmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == o.CompartmentId)?.XPosition)
                                .ThenBy(o => newWmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == o.CompartmentId)?.YPosition);

                        if (OperationRightToLeft)
                        {
                            sortedOperations = newWmsMission.Operations.OrderBy(o => o.Type is MissionOperationType.Pick || o.Type is MissionOperationType.Put ? 0 : 1)
                                //.ThenBy(o => (this.ActiveWmsOperation?.Id == 0 || o.Id == this.ActiveWmsOperation?.Id) ? 0 : 1)
                                .ThenBy(o => o.RowSeq)
                                .ThenBy(o => o.Priority)
                                //.ThenBy(o => o.Status is MissionOperationStatus.Completed || newWmsMission.LoadingUnit == null ? 0 : o.CompartmentId);
                                .ThenByDescending(o => newWmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == o.CompartmentId)?.XPosition)
                                .ThenBy(o => newWmsMission.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == o.CompartmentId)?.YPosition);
                        }

                        // ONLY FOR DEBUG
                        //var newWmsMission2 = await this.missionsWebService.GetWmsDetailsByIdAsync(newMachineMission.WmsId.Value);
                        //var b = sortedOperations.Where(o => o.Status is MissionOperationStatus.Executing).ToList();
                        //try
                        //{
                        //    var cb0 = newWmsMission2.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == b[0].CompartmentId);
                        //    var cb1 = newWmsMission2.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == b[1].CompartmentId);
                        //    var cb2 = newWmsMission2.LoadingUnit.Compartments.FirstOrDefault(c => c.Id == b[2].CompartmentId);
                        //}
                        //catch (Exception)
                        //{
                        //}

                        this.MaxOperation = sortedOperations.Count(o => o.Status == MissionOperationStatus.Executing || o.Status == MissionOperationStatus.New);

                        newWmsOperationInfo = sortedOperations.Where(o => o.Status is MissionOperationStatus.Executing && o.RequestedQuantity > 0).Skip(this.CurrentOperation - 1).FirstOrDefault();

                        if (newWmsOperationInfo is null)
                        {
                            newWmsOperationInfo = sortedOperations.FirstOrDefault(o => o.Status is MissionOperationStatus.New);
                        }

                        if (newWmsOperationInfo is null)
                        {
                            this.logger.Debug($"Active WMS mission '{newMachineMission.WmsId}' has no executable mission operation.");

                            //this.logger.Debug($"Recalling loading unit '{newMachineMission.LoadUnitId}'.");

                            //await this.loadingUnitsWebService.RemoveFromBayAsync(newMachineMission.LoadUnitId);

                            newMachineMission = null;
                            newWmsMission = null;
                        }
                        else
                        {
                            if (configuration.IsAsendia)
                            {
                                try
                                {
                                    await this.missionOperationsWebService.SendIdOperationAsync(newWmsOperationInfo.Id);
                                }
                                catch (Exception)
                                {
                                }
                            }

                            this.logger.Debug($"Active mission has WMS operation {newWmsOperationInfo.Id}; priority {newWmsOperationInfo.Priority}; creation date {newWmsOperationInfo.CreationDate}; status {newWmsOperationInfo.Status}.");

                            try
                            {
                                // is aggregatelist
                                if (await this.identityService.GetAggregateListAsync()
                                    && newWmsOperationInfo.Type == MissionOperationType.Pick)// || newWmsOperationInfo.Type == MissionOperationType.Put)
                                {
                                    newWmsOperation = await this.missionOperationsWebService.GetByAggregateAsync(newWmsOperationInfo.Id);
                                    if (newWmsOperation != null)
                                    {
                                        newWmsOperation.MissionId = newWmsMission.Id;
                                    }
                                }
                                else
                                {
                                    newWmsOperation = await this.missionOperationsWebService.GetByIdAsync(newWmsOperationInfo.Id);
                                }

                                if (newWmsOperation == null || newWmsOperation.Status is MissionOperationStatus.Completed)
                                {
                                    this.logger.Debug($"Active WMS operation '{newWmsOperationInfo.Id}' is closed.");
                                    newMachineMission = null;
                                    newWmsMission = null;
                                    newWmsOperation = null;
                                }
                                else
                                {
                                    await this.missionOperationsWebService.ExecuteAsync(newWmsOperationInfo.Id, this.authenticationService.UserName);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.logger.Debug($"Active WMS mission '{newMachineMission.WmsId}' has failed activation {ex.Message}.");

                                newMachineMission = null;
                                newWmsMission = null;
                                newWmsOperation = null;
                            }
                        }
                    }
                    else
                    {
                        this.logger.Trace($"No Active mission.");

                        this.multilist = false;
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
                       //(newWmsMission != null && this.ActiveWmsMission?.Operations.Any(mo => newWmsMission.Operations.Any(nOp => nOp.Id != mo.Id)) == true)
                       //||
                       missionId.HasValue
                       ||
                       force)
                    {
                        this.ActiveMachineMission = newMachineMission;
                        this.ActiveWmsMission = newWmsMission;
                        this.ActiveWmsOperation = newWmsOperation;

                        this.RaiseMissionChangedEvent();
                    }
                    else
                    {
                        this.logger.Trace($"No mission changed.");
                    }
                    this._semaphore.Release();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    this._semaphore.Release();
                }
            }
        }

        private async Task<Mission> RetrieveActiveMissionAsync(int? missionId = null)
        {
            this.logger.Trace("Retrieving active mission ...");

            try
            {
                // Retrieve properties of bay: check if it is an internal double bay
                var bay = this.machineService.Bay;
                //var isInternalDoubleBay = bay.IsDouble && (bay.Carousel == null);
                //var isExternalDoubleBay = bay.IsDouble && bay.IsExternal;

                // Retrieve the machine missions
                IOrderedEnumerable<Mission> activeMissions = null;

                if (missionId.HasValue)
                {
                    this.machineMissions = await this.missionsWebService.GetAllAsync();
                    activeMissions = this.machineMissions.Where(m =>
                            m.Step is MissionStep.WaitPick
                            &&
                            m.TargetBay == this.bayNumber
                            &&
                            m.Status == MissionStatus.Waiting
                            &&
                            m.ErrorCode == MachineErrorCode.NoError
                            &&
                            m.Id == missionId.Value)
                        .OrderBy(o => o.LoadUnitDestination)
                        .ThenBy(o => o.Priority);
                }
                else
                {
                    this.machineMissions = this.machineService.Missions;
                    activeMissions = this.machineMissions.Where(m =>
                        m.Step is MissionStep.WaitPick
                        &&
                        m.TargetBay == this.bayNumber
                        &&
                        m.Status == MissionStatus.Waiting
                        &&
                        m.ErrorCode == MachineErrorCode.NoError
                        )
                        .OrderBy(o => o.LoadUnitDestination)
                        .ThenBy(o => o.Priority);

                    //else
                    //{
                    //    // Retrieve the active missions according to the enlisted condition.
                    //    // The missions are ordered by creation date (descending way)
                    //    activeMissions = machineMissions.Where(m =>
                    //        m.Step is MissionStep.WaitPick
                    //        &&
                    //        m.TargetBay == this.bayNumber
                    //        &&
                    //        m.Status == MissionStatus.Waiting)
                    //        .OrderByDescending(d => d.CreationDate);
                    //}
                }

                if (activeMissions.Any())
                {
                    var loadUnitId = activeMissions.FirstOrDefault().LoadUnitId;
                    if (this.machineMissions.Any(m => m.LoadUnitId == loadUnitId && m.MissionType == MissionType.IN)
                        // BID must wait for second load unit to move away from bay
                        || (bay.IsDouble && !bay.IsExternal && bay.Carousel == null
                            && bay.Positions.Any(p => p.LoadingUnit != null && p.LoadingUnit.Id != loadUnitId)
                            )
                        )
                    {
                        this.logger.Trace("No active mission on bay - IN mission already present");
                        activeMissions = null;
                    }
                    else
                    {
                        this.logger.Debug($"Active mission has id {activeMissions.FirstOrDefault().Id}");
                    }
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

        private void ThenBy(Func<object, int> value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
