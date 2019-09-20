using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.BackgroundServices
{
    internal partial class MissionsManagerService : AutomationBackgroundService
    {

        #region Fields

        private readonly List<IFiniteStateMachine> activeStateMachines;

        private readonly AutoResetEvent bayStatusChangedEvent = new AutoResetEvent(false);

        private readonly IMachinesDataService machinesDataService;

        private readonly Task missionManagementTask;

        private readonly IMissionsDataService missionsDataService;

        private readonly AutoResetEvent newMissionArrivedResetEvent = new AutoResetEvent(false);

        private readonly IServiceScope serviceScope;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool isDisposed;

        #endregion

        #region Constructors

        public MissionsManagerService(
            IEventAggregator eventAggregator,
            ILogger<MissionsManagerService> logger,
            IMachinesDataService machinesDataService,
            IMissionsDataService missionsDataService,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger)
        {
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.activeStateMachines = new List<IFiniteStateMachine>();

            this.serviceScope = serviceScopeFactory.CreateScope();

            this.missionManagementTask = new Task(async () => await this.ScheduleMissionsOnBaysAsync());

            this.Logger.LogTrace("Mission manager initialized.");
        }

        #endregion



        #region Methods

        public override void Dispose()
        {
            base.Dispose();

            if(!this.isDisposed)
            {
                this.serviceScope.Dispose();

                this.isDisposed = true;
            }
        }

        protected override void NotifyError(IMessageData notificationData)
        {

            this.Logger.LogDebug($"Notifying Mission Manager service error");

            var msg = new NotificationMessage(
                notificationData,
                "MM Error",
                MessageActor.Any,
                MessageActor.MissionsManager,
                MessageType.FsmException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        #endregion
    }
}
