using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
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

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            if (this.dataLayerService.IsReady)
            {
                await this.OnDataLayerReadyAsync();
            }
            else
            {
                this.EventAggregator.GetEvent<NotificationEvent>().Subscribe(async (x) =>
                    await this.OnDataLayerReadyAsync(),
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type is CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);
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
                        .Where(m => m.Status == MissionStatus.New);

                    if (newWmsMissions.Any())
                    {
                        this.Logger.LogDebug("A total of {newMissionsCount} new WMS mission(s) are available.", newWmsMissions.Count());
                    }

                    foreach (var wmsMission in newWmsMissions.Where(m => m.Operations.Any(o => o.Status == MissionOperationStatus.Executing)))
                    {
                        var bayNumber = (CommonUtils.Messages.Enumerations.BayNumber)wmsMission.BayId.Value;
                        try
                        {
                            var bay = baysDataProvider.GetByNumber(bayNumber);

                            missionSchedulingProvider.QueueBayMission(
                                wmsMission.LoadingUnitId,
                                bay.Number,
                                wmsMission.Id,
                                wmsMission.Priority);
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
                            && (m.Status == MissionStatus.Completed
                                || (m.Operations?.All(op => (int)op.Status == (int)CommonUtils.Messages.Enumerations.MissionOperationStatus.OnHold) ?? false))
                                )
                            ).ToArray();

                    foreach (var localMissionToAbort in localMissionsToAbort)
                    {
                        missionSchedulingProvider.AbortMission(localMissionToAbort);
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
