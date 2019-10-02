using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Hubs.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
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

        private readonly IDataHubClient dataHubClient;

        private readonly IHubContext<InstallationHub, IInstallationHub> installationHub;

        private readonly IMachinesDataService machinesDataService;

        private readonly IMissionsDataService missionDataService;

        private readonly IHubContext<OperatorHub, IOperatorHub> operatorHub;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private List<DataModels.Bay> configuredBays;

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

            this.installationHub = installationHub ?? throw new ArgumentNullException(nameof(installationHub));
            this.dataHubClient = dataHubClient ?? throw new ArgumentNullException(nameof(dataHubClient));
            this.machinesDataService = machinesDataService ?? throw new ArgumentNullException(nameof(machinesDataService));
            this.operatorHub = operatorHub ?? throw new ArgumentNullException(nameof(operatorHub));
            this.baysDataService = baysDataService ?? throw new ArgumentNullException(nameof(baysDataService));
            this.missionDataService = missionDataService ?? throw new ArgumentNullException(nameof(missionDataService));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
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
