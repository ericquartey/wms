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

        private readonly IConfiguration configuration;

        private readonly IDataLayerService dataLayerService;

        private readonly IMachinesWmsWebService machinesWmsWebService;

        private bool dataLayerIsReady;

        private int machineId;

        #endregion

        #region Constructors

        public WmsMissionProxyService(
            IMachinesWmsWebService machinesWmsWebService,
            IConfiguration configuration,
            IDataLayerService dataLayerService,
            IEventAggregator eventAggregator,
            ILogger<WmsMissionProxyService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesWmsWebService = machinesWmsWebService ?? throw new ArgumentNullException(nameof(machinesWmsWebService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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
            if (!this.configuration.IsWmsEnabled() || !this.dataLayerIsReady)
            {
                return;
            }

            this.Logger.LogDebug("Checking for new WMS missions ...");

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();

                // 1. Get all missions from WMS
                var wmsMissions = await this.machinesWmsWebService.GetMissionsByIdAsync(this.machineId);

                // 2. Get all known WMS missions (already recorded in the local database)
                var localMissions = missionsDataProvider.GetAllWmsMissions();

                // 3. Select the new unknown WMS missions and queue them
                var newWmsMissions = wmsMissions
                    .Where(m => m.BayId.HasValue)
                    .Where(m => localMissions.All(lm => lm.WmsId != m.Id))
                    .Where(m => m.Status == MissionStatus.New);

                this.Logger.LogDebug(newWmsMissions.Any()
                    ? "A total of {newMissionsCount} new WMS mission(s) are available."
                    : "No new WMS missions are available.", newWmsMissions.Count());

                foreach (var wmsMission in newWmsMissions)
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

                //// 4. Select the known missions that were aborted on WMS and abort them
                //var localMissionsToAbort = localMissions
                //    .Where(m => m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Completed)
                //    .Where(m => wmsMissions.Any(m1 => m1.Id == m.WmsId && m1.Status == MissionStatus.Completed)).ToArray();

                //foreach (var localMissionToAbort in localMissionsToAbort)
                //{
                //    missionSchedulingProvider.AbortMission(localMissionToAbort);
                //}
            }
        }

        #endregion
    }
}
