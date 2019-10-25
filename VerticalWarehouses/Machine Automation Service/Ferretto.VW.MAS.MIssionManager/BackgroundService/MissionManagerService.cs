﻿using System;
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

namespace Ferretto.VW.MAS.MissionManager.BackgroundService
{
    internal partial class MissionManagerService : AutomationBackgroundService<CommandMessage, NotificationMessage, CommandEvent, NotificationEvent>
    {
        #region Fields

        private readonly AutoResetEvent bayStatusChangedEvent = new AutoResetEvent(false);

        private readonly IConfiguration configuration;

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        #endregion

        #region Constructors

        public MissionManagerService(
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            ILogger<MissionManagerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.missionManagementTask = new Task(async () => await this.ScheduleMissionsOnBaysAsync());

            this.Logger.LogTrace("Mission manager initialized.");
        }

        #endregion

        #region Methods

        protected override void NotifyCommandError(CommandMessage notificationData)
        {
            throw new NotImplementedException();
        }

        protected override void NotifyError(NotificationMessage notificationData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
