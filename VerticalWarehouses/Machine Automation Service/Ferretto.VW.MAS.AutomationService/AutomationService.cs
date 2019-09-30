using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.AutomationService.StateMachines.Interface;
using Ferretto.VW.MAS.AutomationService.StateMachines.PowerEnable;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class AutomationService : AutomationBackgroundService
    {

        #region Fields

        private readonly IApplicationLifetime applicationLifetime;

        private readonly IBaysDataService baysDataService;

        private readonly IBaysProvider baysProvider;

        private readonly BlockingConcurrentQueue<CommandMessage> commandQueue;

        private readonly Task commandReceiveTask;

        private readonly IDataHubClient dataHubClient;

        private readonly IEventAggregator eventAggregator;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly ILogger<AutomationService> logger;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionDataService;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private List<DataModels.Bay> configuredBays;

        private IStateMachine currentStateMachine;

        private CancellationToken stoppingToken;

        #endregion

        #region Constructors

        public AutomationService(
            IEventAggregator eventAggregator,
            IHubContext<InstallationHub, IInstallationHub> installationHub,
            ILogger<AutomationService> logger,
            IDataHubClient dataHubClient,
            IMachinesDataService machinesDataService,
            IHubContext<OperatorHub, IOperatorHub> operatorHub,
            IBaysDataService baysDataService,
            IMissionsDataService missionDataService,
            IServiceScopeFactory serviceScopeFactory,
            IApplicationLifetime applicationLifetime,
            IBaysProvider baysProvider)
            : base(eventAggregator, logger)
        {
            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            if (applicationLifetime is null)
            {
                throw new ArgumentNullException(nameof(applicationLifetime));
            }

            if (installationHub is null)
            {
                throw new ArgumentNullException(nameof(installationHub));
            }

            if (dataHubClient is null)
            {
                throw new ArgumentNullException(nameof(dataHubClient));
            }

            if (machinesDataService is null)
            {
                throw new ArgumentNullException(nameof(machinesDataService));
            }

            if (operatorHub is null)
            {
                throw new ArgumentNullException(nameof(operatorHub));
            }

            if (baysDataService is null)
            {
                throw new ArgumentNullException(nameof(baysDataService));
            }

            if (missionDataService is null)
            {
                throw new ArgumentNullException(nameof(missionDataService));
            }

            this.Logger.LogTrace("1:Method Start");

            this.installationHub = installationHub;
            this.dataHubClient = dataHubClient;
            this.machinesDataService = machinesDataService;
            this.operatorHub = operatorHub;
            this.baysDataService = baysDataService;
            this.missionDataService = missionDataService;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.baysProvider = baysProvider;
            this.applicationLifetime = applicationLifetime;
            this.eventAggregator = eventAggregator;
        }

        #endregion



        #region Methods

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            await this.dataHubClient.ConnectAsync();
        }

        protected override void NotifyCommandError(CommandMessage notificationData)
        {

            this.Logger.LogDebug($"Notifying Automation Service service error");

            var msg = new NotificationMessage(
                notificationData.Data,
                "AS Error",
                MessageActor.Any,
                MessageActor.MissionsManager,
                MessageType.FsmException,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationError,
                ErrorLevel.Critical);

            this.EventAggregator.GetEvent<NotificationEvent>().Publish(msg);
        }

        protected override void NotifyError(NotificationMessage notificationData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
