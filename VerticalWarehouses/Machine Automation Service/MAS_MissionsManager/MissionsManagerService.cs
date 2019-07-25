using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager
{
    public partial class MissionsManagerService : AutomationBackgroundService
    {
        #region Fields

        private const int MaximumBaysCount = 3;

        private readonly IList<Bay> bays = new List<Bay>(MaximumBaysCount);

        private readonly AutoResetEvent bayStatusChangedEvent = new AutoResetEvent(false);

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        #endregion

        #region Constructors

        public MissionsManagerService(
            IEventAggregator eventAggregator,
            ILogger<MissionsManagerService> logger,
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService)
            : base(eventAggregator, logger)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (machinesDataService == null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            if (missionsDataService == null)
            {
                throw new ArgumentNullException(nameof(missionsDataService));
            }

            this.eventAggregator = eventAggregator;
            this.machinesDataService = machinesDataService;
            this.missionsDataService = missionsDataService;

            this.missionManagementTask = new Task(async () => await this.ScheduleMissionsOnBaysAsync());

            this.Logger.LogTrace("Mission manager initialised.");
        }

        #endregion
    }
}
