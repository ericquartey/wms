using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
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

        private readonly AutoResetEvent bayStatusChangedEvent = new AutoResetEvent(false);

        private readonly IConfiguration configuration;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMissionsDataService missionsDataService;

        private readonly Task scheduleMissionsOnBaysTask;

        #endregion

        #region Constructors

        public WmsMissionProxyService(
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService,
            IMissionOperationsDataService missionOperationsDataService,
            IMissionSchedulingProvider missionSchedulingProvider,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            ILogger<WmsMissionProxyService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.scheduleMissionsOnBaysTask = new Task(async () => await this.ScheduleMissionsOnBaysAsync());
        }

        #endregion
    }
}
