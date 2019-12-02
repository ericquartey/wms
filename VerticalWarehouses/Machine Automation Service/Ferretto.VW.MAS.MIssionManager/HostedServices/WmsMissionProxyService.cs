using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
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

            this.Logger.LogInformation("Checking for new WMS missions.");

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                var missionsDataProvider = scope.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
                var missionSchedulingProvider = scope.ServiceProvider.GetRequiredService<IMissionSchedulingProvider>();

                // 1. Get all missions from WMS
                var machineId = machineProvider.GetIdentity();
                var wmsMissions = await this.machinesDataService.GetMissionsByIdAsync(machineId);

                // 2. Get all known WMS missions (already recorded in the local database)
                var localMissions = missionsDataProvider.GetAllWmsMissions();

                foreach (var wmsMission in wmsMissions.Where(m => m.BayId.HasValue))
                {
                    if (!localMissions.Any(m => m.WmsId == wmsMission.Id))
                    {
                        // 3. If the mission is not found in database, queue a new mission request
                        missionSchedulingProvider.QueueBayMission(
                            wmsMission.LoadingUnitId,
                            (BayNumber)wmsMission.BayId, // TODO **** careful here: bayId && bayNumber
                            wmsMission.Id,
                            wmsMission.Priority);
                    }
                }
            }
        }

        #endregion
    }
}
