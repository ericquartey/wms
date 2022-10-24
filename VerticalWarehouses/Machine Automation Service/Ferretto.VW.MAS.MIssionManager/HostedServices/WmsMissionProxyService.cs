using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class WmsMissionProxyService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly IDataLayerService dataLayerService;

        private bool dataLayerIsReady;

        private int machineId;

        #endregion

        #region Constructors

        public WmsMissionProxyService(
            IDataLayerService dataLayerService,
            IEventAggregator eventAggregator,
            ILogger<WmsMissionProxyService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.dataLayerService = dataLayerService ?? throw new ArgumentNullException(nameof(dataLayerService));
        }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var dataHubClient = scope.ServiceProvider.GetRequiredService<IDataHubClient>();

                dataHubClient.EntityChanged += async (s, e) => await this.OnWmsEntityChangedAsync(s, e);
                dataHubClient.ConnectionStatusChanged += async (s, e) => await this.OnWmsConnectionStatusChangedAsync(s, e);

                //if (this.dataLayerService.IsReady)
                //{
                //    await this.OnDataLayerReadyAsync(scope.ServiceProvider);
                //}
                //else
                //{
                //    this.EventAggregator.GetEvent<NotificationEvent>().Subscribe(async (x) =>
                //        await this.OnDataLayerReadyAsync(scope.ServiceProvider),
                //        ThreadOption.PublisherThread,
                //        false,
                //        m => m.Type is CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);
                //}
            }
        }

        private void NotifyAssignedMissionChanged(
            BayNumber bayNumber,
            int? missionId)
        {
            var data = new AssignedMissionChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId,
            };

            var notificationMessage = new NotificationMessage(
                data,
                $"Mission assigned to bay {bayNumber} has changed.",
                MessageActor.WebApi,
                MessageActor.MachineManager,
                MessageType.AssignedMissionChanged,
                bayNumber);

            this.EventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        private async Task OnWmsConnectionStatusChangedAsync(object sender, ConnectionStatusChangedEventArgs e)
        {
            this.Logger.LogDebug("Connection to WMS hub changed (connected={isConnected})", e.IsConnected);
            if (e.IsConnected)
            {
                await this.OnWmsEntityChangedAsync(this, new EntityChangedEventArgs(
                    nameof(MissionOperation),
                    null, WMS.Data.Hubs.Models.HubEntityOperation.Created,
                    null,
                    null));
            }
            else
            {
                var scope = base.ServiceScopeFactory.CreateScope();
                await this.OnWmsEnableChanged(scope.ServiceProvider);
            }
        }

        private async Task OnWmsEnableChanged(IServiceProvider serviceProvider)
        {
            this.Logger.LogDebug("Wms connection enable changed");
            var dataHubClient = serviceProvider.GetRequiredService<IDataHubClient>();
            var wmsSettingsProvider = serviceProvider.GetRequiredService<IWmsSettingsProvider>();
            if (wmsSettingsProvider.IsEnabled)
            {
                wmsSettingsProvider.IsConnected = false;
                await dataHubClient.ConnectAsync(new Uri(wmsSettingsProvider.ServiceUrl, "hubs/data"));
            }
            else
            {
                try
                {
                    if (dataHubClient.IsConnected)
                    {
                        await dataHubClient.DisconnectAsync();
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }

        private async Task OnWmsEntityChangedAsync(object sender, EntityChangedEventArgs e)
        {
            if (e.Operation != WMS.Data.Hubs.Models.HubEntityOperation.Created)
            {
                return;
            }

            switch (e.EntityType)
            {
                case nameof(MissionOperation):
                    {
                        var msg = new NotificationMessage(
                            null,
                            "New WMS mission available",
                            MessageActor.Any,
                            MessageActor.AutomationService,
                            MessageType.NewWmsMissionAvailable,
                            BayNumber.None,
                            BayNumber.None,
                            MessageStatus.OperationStart);

                        this.EventAggregator
                            .GetEvent<NotificationEvent>()
                            .Publish(msg);

                        using (var scope = this.ServiceScopeFactory.CreateScope())
                        {
                            try
                            {
                                var missionWebService = scope.ServiceProvider.GetRequiredService<IMissionOperationsWmsWebService>();
                                var missionDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                                if (int.TryParse(e.Id, out var operationId))
                                {
                                    var operation = await missionWebService.GetByIdAsync(operationId);
                                    var mission = missionDataProvider.GetByWmsId(operation.MissionId);
                                    this.NotifyAssignedMissionChanged(mission.TargetBay, null);
                                }
                            }
                            catch
                            {
                                // do nothing
                            }
                        }
                        break;
                    }
            }
        }

        private void RetrieveMachineId()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                this.machineId = machineProvider.GetIdentity();
            }
        }

        private async Task RetrieveNewWmsMissionsAsync()
        {
            if (!this.dataLayerIsReady)
            {
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var wmsSettingsProvider = scope.ServiceProvider.GetRequiredService<IWmsSettingsProvider>();
                if (!wmsSettingsProvider.IsEnabled
                    || !wmsSettingsProvider.IsConnected
                    )
                {
                    return;
                }

                var machinemachineVolatileDataProvider = scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                if (machinemachineVolatileDataProvider.Mode != MachineMode.Automatic
                    //&& machinemachineVolatileDataProvider.Mode != MachineMode.SwitchingToAutomatic
                    )
                {
                    return;
                }

                this.Logger.LogDebug("Checking for new WMS missions ...");

                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();

                try
                {
                    // 1. Get all missions from WMS
                    var machinesWmsWebService = scope.ServiceProvider.GetRequiredService<IMachinesWmsWebService>();
                    var wmsMissions = await machinesWmsWebService.GetMissionsByIdAsync(this.machineId);

                    // 2. Get all known WMS missions (already recorded in the local database)
                    var localMissions = missionsDataProvider.GetAllWmsMissions();

                    // 3. Select the new unknown WMS missions and queue them
                    var newWmsMissions = wmsMissions
                        .Where(m => m.BayId.HasValue)
                        .Where(m => localMissions.All(lm => lm.WmsId != m.Id))
                        .Where(m => m.Status == WMS.Data.WebAPI.Contracts.MissionStatus.New);

                    if (newWmsMissions.Any())
                    {
                        this.Logger.LogDebug("A total of {newMissionsCount} new WMS mission(s) are available.", newWmsMissions.Count());
                    }

                    foreach (var wmsMission in newWmsMissions.Where(m => m.Operations.Any(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.Executing))
                                                                .OrderBy(m => m.Operations != null ? m.Operations.Min(o => o.Priority) : 0)
                                                                .ThenBy(m => m.CreationDate))
                    {
                        var bayNumber = (CommonUtils.Messages.Enumerations.BayNumber)wmsMission.BayId.Value;
                        try
                        {
                            var bay = baysDataProvider.GetByNumber(bayNumber);

                            //if (!bay.Inventory && (wmsMission.Operations.Count() == wmsMission.Operations.Where(s => s.Type == MissionOperationType.Inventory).Count()))
                            //{
                            //    throw new InvalidOperationException($"Mission not allowed in this bay");
                            //}
                            //else if (!bay.Pick && (wmsMission.Operations.Count() == wmsMission.Operations.Where(s => s.Type == MissionOperationType.Pick).Count()))
                            //{
                            //    throw new InvalidOperationException($"Mission not allowed in this bay");
                            //}
                            //else if (!bay.Put && (wmsMission.Operations.Count() == wmsMission.Operations.Where(s => s.Type == MissionOperationType.Put).Count()))
                            //{
                            //    throw new InvalidOperationException($"Mission not allowed in this bay");
                            //}
                            //else if (!bay.View && (wmsMission.Operations.Count() == wmsMission.Operations.Where(s => s.Type == (MissionOperationType)Enum.Parse(typeof(MissionOperationType), "4")).Count()))
                            //{
                            //    throw new InvalidOperationException($"Mission not allowed in this bay");
                            //}

                            var waitMission = localMissions.FirstOrDefault(m => m.Status == CommonUtils.Messages.Enumerations.MissionStatus.Waiting
                                                 && m.TargetBay == bayNumber
                                                 && m.LoadUnitId == wmsMission.LoadingUnitId
                                                 && wmsMissions.Any(w => w.Id == m.WmsId
                                                     && (w.Operations?.All(op => op.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.Completed
                                                         || op.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.Suspended) ?? false)));
                            if (waitMission is null)
                            {
                                missionSchedulingProvider.QueueBayMission(
                                    wmsMission.LoadingUnitId,
                                    bay.Number,
                                    wmsMission.Id,
                                    wmsMission.Priority);
                            }
                            else
                            {
                                waitMission.WmsId = wmsMission.Id;
                                missionsDataProvider.Update(waitMission);
                                this.Logger.LogInformation($"Update MAS bay mission from WMS mission id={wmsMission.Id} for load unit {wmsMission.LoadingUnitId}");
                            }
                        }
                        catch (EntityNotFoundException)
                        {
                            var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                            errorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, bayNumber, $"{ErrorDescriptions.LoadUnitNumber} {wmsMission.LoadingUnitId}");
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError("Unable to queue mission on bay '{bayNumber}': '{details}'.", bayNumber, ex.Message);
                        }
                    }

                    // 4. Select the known missions that were aborted/completed/suspended on WMS and abort them
                    var localMissionsToAbort = localMissions
                        .Where(lm => lm.Status == CommonUtils.Messages.Enumerations.MissionStatus.New)
                        .Where(lm => wmsMissions.Any(m => m.Id == lm.WmsId
                            && (m.Status == WMS.Data.WebAPI.Contracts.MissionStatus.Completed
                                || (m.Operations?.All(op => (int)op.Status == (int)CommonUtils.Messages.Enumerations.MissionOperationStatus.OnHold) ?? false))
                                )
                            ).ToArray();

                    foreach (var localMissionToAbort in localMissionsToAbort)
                    {
                        missionSchedulingProvider.AbortMission(localMissionToAbort);
                    }

                    // 5. check the bay missions that were aborted/completed/suspended on WMS - they are managed by the ScheduleWmsMissionAsync
                    var bayMissionsToAbort = localMissions
                        .Any(lm => lm.Status == CommonUtils.Messages.Enumerations.MissionStatus.Waiting
                            && wmsMissions.Any(m => m.Id == lm.WmsId
                            && (m.Status == WMS.Data.WebAPI.Contracts.MissionStatus.Completed
                                || (m.Operations?.All(op => (int)op.Status == (int)CommonUtils.Messages.Enumerations.MissionOperationStatus.OnHold) ?? false))
                                )
                            );

                    if (bayMissionsToAbort)
                    {
                        var notificationMessage = new NotificationMessage(
                            null,
                            $"New machine mission available for bay {BayNumber.BayOne}.",
                            MessageActor.MissionManager,
                            MessageActor.MissionManager,
                            MessageType.NewMachineMissionAvailable,
                            BayNumber.BayOne);

                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogWarning("Unable to retrieve WMS missions: {details}", ex.Message);
                }
            }
        }

        #endregion
    }
}
