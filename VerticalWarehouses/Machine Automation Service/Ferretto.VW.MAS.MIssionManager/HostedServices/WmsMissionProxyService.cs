using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer;
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

        private readonly IMachinesDataService machinesDataService;

        private bool dataLayerIsReady;

        #endregion

        #region Constructors

        public WmsMissionProxyService(
            IMachinesDataService machinesDataService,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            ILogger<WmsMissionProxyService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Methods

        private async Task RetrieveNewWmsMissionsAsync()
        {
            if (!this.configuration.IsWmsEnabled() || !this.dataLayerIsReady)
            {
                return;
            }

            this.Logger.LogDebug("Checking for new WMS missions ...");

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();

                // 1. Get all missions from WMS
                var machineId = machineProvider.GetIdentity();
                var wmsMissions = await this.machinesDataService.GetMissionsByIdAsync(machineId);

                // 2. Get all known WMS missions (already recorded in the local database)
                var localMissions = missionsDataProvider.GetAllWmsMissions();

                // 3. Select the new unknown WMS missions and queue them
                var newWmsMissions = wmsMissions
                    .Where(m => m.BayId.HasValue)
                    .Where(m => !localMissions.Any(lm => lm.WmsId == m.Id));

                this.Logger.LogDebug("A total of {newMissionsCount} is available", newWmsMissions.Count());

                foreach (var wmsMission in newWmsMissions)
                {
                    var bay = baysDataProvider.GetByIdOrDefault(wmsMission.BayId.Value);

                    if (bay is null)
                    {
                        this.Logger.LogWarning($"The WMS mission id={wmsMission.Id} cannot be accepted because no bay with id={wmsMission.BayId.Value} exists on the machine.");
                    }
                    else
                    {
                        missionSchedulingProvider.QueueBayMission(
                            wmsMission.LoadingUnitId,
                            bay.Number,
                            wmsMission.Id,
                            wmsMission.Priority);
                    }
                }

                // 4. Select the known missions that were aborted on WMS and abort them
                var localMissionsToAbort = localMissions
                    .Where(m => m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Completed)
                    .Where(m => wmsMissions.Any(m1 => m1.Id == m.WmsId && m1.Status == MissionStatus.Completed));

                foreach (var localMissionToAbort in localMissionsToAbort)
                {
                    missionSchedulingProvider.AbortMission(localMissionToAbort);
                }
            }
        }

        #endregion
    }
}
