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

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.MissionsManager
{
    internal partial class MissionsManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly AutoResetEvent bayStatusChangedEvent = new AutoResetEvent(false);

        private readonly IConfiguration configuration;

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        private readonly IServiceScope serviceScope;

        private bool isDisposed;

        #endregion

        #region Constructors

        public MissionsManagerService(
            IEventAggregator eventAggregator,
            ILogger<MissionsManagerService> logger,
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.serviceScope = serviceScopeFactory.CreateScope();
            this.missionManagementTask = new Task(async () => await this.ScheduleMissionsOnBaysAsync());

            this.Logger.LogTrace("Mission manager initialised.");
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();

            if (!this.isDisposed)
            {
                this.serviceScope.Dispose();

                this.isDisposed = true;
            }
        }

        #endregion
    }
}
